using Microsoft.AspNetCore.Identity;
using RSS_DB.Entities;

namespace RSS
{
    public static class AdminRoleSeeder
    {
        public const string AdminRoleName = "Admin";

        public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync(AdminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
                logger.LogInformation("Created '{Role}' role", AdminRoleName);
            }

            var seedEmail = config["Admin:SeedEmail"];
            if (string.IsNullOrWhiteSpace(seedEmail))
                return;

            var user = await userManager.FindByEmailAsync(seedEmail);
            if (user == null)
            {
                logger.LogWarning("Admin seed email '{Email}' not found; no admin assigned", seedEmail);
                return;
            }

            if (await userManager.IsInRoleAsync(user, AdminRoleName))
                return;

            await userManager.AddToRoleAsync(user, AdminRoleName);
            // Invalidate existing tokens so the user picks up the role claim on next login
            await userManager.UpdateSecurityStampAsync(user);
            logger.LogInformation("Assigned '{Role}' role to {Email}", AdminRoleName, seedEmail);
        }
    }
}
