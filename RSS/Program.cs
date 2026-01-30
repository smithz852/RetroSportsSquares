using Microsoft.EntityFrameworkCore;
using RSS.SportsDataAutomation;
using RSS_DB;
using RSS_Services;
using RSS_Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<RSS_Services.AvailableGamesServices>();
builder.Services.AddScoped<NbaDataPullHelper>();
builder.Services.AddScoped<FootballMapperHelper>();
builder.Services.AddScoped<GeneralServices>();
builder.Services.AddScoped<TimeHelpers>();
builder.Services.AddScoped<DataSortHelpers>();
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        sql => sql.MigrationsAssembly("RSS_DB")
    )
);
builder.Services.AddHostedService<NflAutomation>();
builder.Services.AddHostedService<NbaAutomation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
