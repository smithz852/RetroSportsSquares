using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RSS.SportsDataAutomation;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services;
using RSS_Services.Helpers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
});

// Your existing services
builder.Services.AddScoped<RSS_Services.AvailableGamesServices>();
builder.Services.AddScoped<BasketballMapperHelper>();
builder.Services.AddScoped<FootballMapperHelper>();
builder.Services.AddScoped<GeneralServices>();
builder.Services.AddScoped<TimeHelpers>();
builder.Services.AddHttpClient<RSS_Services.SportsGameServices>(client =>
{
    client.DefaultRequestHeaders.Add("x-apisports-key", "2f14287fb764f299801970b51492fe7e");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", "v1.american-football.api-sports.io");
});
builder.Services.AddScoped<RSS.Helpers.MapperHelpers>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<NflAutomation>();
builder.Services.AddHostedService<NbaAutomation>();
builder.Services.AddHostedService<NflRefetchAutomation>();
builder.Services.AddHostedService<BasketballRefetchAutomation>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
