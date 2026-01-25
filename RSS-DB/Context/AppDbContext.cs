using Microsoft.EntityFrameworkCore;
using RSS_DB.Entities;

namespace RSS_DB
{
    public class AppDbContext : DbContext
    {
        //Migration commands from API dir in dev powershell
        //dotnet ef migrations add {NAME_HERE} --project ../RSS-DB --startup-project
        //dotnet ef database update --project ../RSS-DB --startup-project
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AvailableGames> AvailableGames { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<AvailableGames>()
        //        .HasKey(e => e.GameId);
        //}
    }
}
