using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Resend;
using RSS;
using RSS.Middleware;
using RSS.SportsDataAutomation;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.Helpers;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using ZlEmailProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        sql => sql.MigrationsAssembly("RSS_DB")
    )
);

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1);
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    // SignalR sends the token via query string on the WebSocket handshake
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) &&
                ctx.Request.Path.StartsWithSegments("/hubs"))
                ctx.Token = token;
            return Task.CompletedTask;
        },
        OnTokenValidated = async ctx =>
        {
            var userId = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var stampClaim = ctx.Principal?.FindFirstValue("security_stamp");

            if (userId == null || stampClaim == null)
            {
                ctx.Fail("Missing required claims");
                return;
            }

            var cache = ctx.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = $"stamp_{userId}";

            if (!cache.TryGetValue(cacheKey, out string? cachedStamp))
            {
                var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    ctx.Fail("User not found");
                    return;
                }

                cachedStamp = user.SecurityStamp;
                cache.Set(cacheKey, cachedStamp, TimeSpan.FromSeconds(30));
            }

            if (cachedStamp != stampClaim)
                ctx.Fail("Security stamp mismatch");
        }
    };
});

// Your existing services
builder.Services.AddScoped<RSS_Services.AvailableGamesServices>();
builder.Services.AddScoped<BasketballMapperHelper>();
builder.Services.AddScoped<FootballMapperHelper>();
builder.Services.AddScoped<SoccerMapperHelper>();
builder.Services.AddScoped<GeneralServices>();
builder.Services.AddScoped<TimeHelpers>();
builder.Services.AddScoped<RSS_Services.SquareServices>();
builder.Services.AddScoped<RSS_Services.GamePlayerServices>();
builder.Services.AddScoped<RSS_Services.PlayerDashboardService>();
builder.Services.AddScoped<RSS_Services.AdminDashboardService>();
builder.Services.AddScoped<RSS_Services.UserServices>();
builder.Services.AddScoped<RSS_Services.ChatServices>();
builder.Services.AddScoped<RSS_Services.WalletService>();
builder.Services.AddScoped<RSS_Services.GameNotificationService>();
builder.Services.AddScoped<RSS_Services.GameResultProcessor>();
builder.Services.AddHttpClient<RSS_Services.SportsGameServices>(client =>
{
    client.DefaultRequestHeaders.Add("x-apisports-key", "2f14287fb764f299801970b51492fe7e");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", "v1.american-football.api-sports.io");
});
builder.Services.AddScoped<RSS.Helpers.MapperHelpers>();
builder.Services.AddScoped<RSS_Services.IGameHubNotifier, RSS.Hubs.GameHubNotifier>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste the JWT from /auth/login (no 'Bearer ' prefix needed)"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHostedService<BasketballAutomation>();
builder.Services.AddHostedService<FootballAutomation>();
builder.Services.AddHostedService<SoccerAutomation>();
builder.Services.AddHostedService<FootballRefetchAutomation>();
builder.Services.AddHostedService<BasketballRefetchAutomation>();
builder.Services.AddHostedService<SoccerRefetchAutomation>();

//email services
builder.Services.AddResend(o => o.ApiToken = builder.Configuration["Resend:ApiKey"]);
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<TokenService>();

// Rate limiting
var authPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "/auth/forgot-password",
    "/auth/reset-password",
    "/user/request-email-change"
};

builder.Services.AddRateLimiter(options =>
{
    var ipLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (!authPaths.Contains(context.Request.Path.Value ?? ""))
            return RateLimitPartition.GetNoLimiter("no-limit");

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"ip:{ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    var emailLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (!authPaths.Contains(context.Request.Path.Value ?? ""))
            return RateLimitPartition.GetNoLimiter("no-limit");

        var key = context.Items["rate-limit-key"] as string ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"email:{key}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0
            });
    });

    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(ipLimiter, emailLimiter);

    options.AddPolicy("chat-send", context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"chat:{userId}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many requests. Please try again later." }, token);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<EmailExtractionMiddleware>(); // must run after auth so JWT claims are available
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<RSS.Hubs.GameHub>("/hubs/game");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DevDataSeeder.Seed(db);
}

using (var scope = app.Services.CreateScope())
{
    await AdminRoleSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}

app.Run();
