using Microsoft.AspNetCore.Identity;
using untitled1.Models.Entities;

namespace untitled1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Create roles if they don't exist
            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin accounts (idempotent — skips if already exists)
            var admins = new[]
            {
                new { Email = "admin1@filmix.com", FullName = "Admin 1" },
                new { Email = "admin2@filmix.com", FullName = "Admin 2" },
            };

            foreach (var a in admins)
            {
                if (await userManager.FindByEmailAsync(a.Email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = a.Email,
                    Email = a.Email,
                    FullName = a.FullName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
