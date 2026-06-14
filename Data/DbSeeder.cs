using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using untitled1.Models.Entities;

namespace untitled1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger? logger = null)
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

            var adminPassword = Environment.GetEnvironmentVariable("FILMIX_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                // ── Option A: Development Fallback (H-02) ────────────────────────────
                // FILMIX_ADMIN_PASSWORD chưa được cấu hình.
                // Sử dụng fallback password cho môi trường phát triển/đồ án.
                // KHÔNG sử dụng fallback này trong môi trường Production thực tế.
                adminPassword = "FilmixAdmin@Secure2026!";
                var warnMsg = "[WARN] FILMIX_ADMIN_PASSWORD not configured. " +
                              "Using development fallback admin password. " +
                              "DO NOT use this in a real production environment.";
                if (logger != null)
                    logger.LogWarning(warnMsg);
                else
                    Console.WriteLine(warnMsg);
            }

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

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
