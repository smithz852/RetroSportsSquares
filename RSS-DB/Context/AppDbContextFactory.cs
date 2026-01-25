using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_DB.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql(
                "Server=localhost;Database=RetroSportsSquares;Uid=root;Pwd=1234ABc#;",
                new MySqlServerVersion(new Version(8, 0, 21))
            );
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
