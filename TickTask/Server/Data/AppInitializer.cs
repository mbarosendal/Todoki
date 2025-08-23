using Microsoft.AspNetCore.Identity;
using TickTask.Server.Data.Helpers;
using TickTask.Server.Data.Models;

namespace TickTask.Server.Data
{
    public static class AppDbInitializer
    {

        // Seed roles
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        }

        // Seed default admin
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILogger _logger)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (userManager == null || roleManager == null)
            {
                _logger.LogError("Failed to retrieve required services (UserManager or RoleManager)");
                return;
            }

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            var adminEmail = "admin@admin.com";
            var adminPassword = configuration["Admin:Password"];

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            //_logger.LogWarning("SeedAdminAsync: Default admin user already exists. Aborting initializer setup...");

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    //UserName = adminEmail,
                    Email = adminEmail,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, UserRoles.Admin);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create admin: {Errors}", errors);
                }
            }
        }
    }
}
