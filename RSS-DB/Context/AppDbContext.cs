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
        public DbSet<Squares> Squares { get; set; }

        private static Guid CreateGuid(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var squares = new List<Squares>();

            // 1️⃣ Top numbers (top-0 → top-9)
            for (int i = 0; i < 10; i++)
            {
                var name = $"top-{i}";
                squares.Add(new Squares
                {
                    Id = CreateGuid(name),
                    Name = name
                });
            }

            // 2️⃣ Row numbers (row-0 → row-9)
            for (int i = 0; i < 10; i++)
            {
                var name = $"row-{i}";
                squares.Add(new Squares
                {
                    Id = CreateGuid(name),
                    Name = name
                });
            }

            // 3️⃣ Main 10x10 grid (0-0 → 9-9)
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    var name = $"{row}-{col}";
                    squares.Add(new Squares
                    {
                        Id = CreateGuid(name),
                        Name = name
                    });
                }
            }

            builder.Entity<Squares>().HasData(squares);
        }
    }
}
