using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RSS_DB.Entities;
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



        private static Guid CreateGuid(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

           

            var nbaGameSeeds = new List<DailySportsGames>
            {
                new DailySportsGames
                {
                    Id = CreateGuid("nba-game-1"),
                    ApiGameId = 12345,
                    HomeTeam = "Lakers",
                    AwayTeam = "Warriors",
                    GameStartTime = DateTimeOffset.UtcNow,
                    SportType = "basketball",
                    League = "NBA",
                    LeagueId = 12,
                    Status = "NS",
                    InUse = false
                },
                // Add more games...
            };

            builder.Entity<DailySportsGames>().HasData(nbaGameSeeds);
        }
    }
}
