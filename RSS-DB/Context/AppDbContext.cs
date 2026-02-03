using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RSS_DB.Entities;

namespace RSS_DB
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        //Migration commands from API dir in dev powershell
        //dotnet ef migrations add {NAME_HERE} --project ../RSS-DB --startup-project .
        //dotnet ef database update --project ../RSS-DB --startup-project .
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AvailableGames> AvailableGames { get; set; }
        public DbSet<DailySportsGames> DailySportsGames { get; set; }
    }
}
