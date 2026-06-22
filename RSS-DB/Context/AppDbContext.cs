using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RSS_DB.Entities;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RSS_DB
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        //Migration commands from API dir in dev powershell
        //dotnet ef migrations add initialCreate --project ../RSS-DB --startup-project .
        //dotnet ef database update --project ../RSS-DB --startup-project .

        //for seed migrations
        //dotnet ef migrations add SeedData --project ../RSS-DB --startup-project .
        //dotnet ef database update --project ../RSS-DB --startup-project .
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SquareGames> SquareGames { get; set; }
        public DbSet<DailySportsGames> DailySportsGames { get; set; }
        public DbSet<GamePlayer> GamePlayers { get; set; }
        public DbSet<GameSquares> GameSquares { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var periodWinnersComparer = new ValueComparer<Dictionary<int, string?>>(
                (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                v => JsonSerializer.Deserialize<Dictionary<int, string?>>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new()
            );

            builder.Entity<SquareGames>()
                .Property(g => g.PeriodWinners)
                .HasColumnType("longtext")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<int, string?>>(v, (JsonSerializerOptions?)null) ?? new()
                )
                .Metadata.SetValueComparer(periodWinnersComparer);
        }
    }
}
