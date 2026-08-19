using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SentinelCase.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole<Guid>>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var roles = new[]
        {
            "Analyst",
            "SocManager",
            "Administrator"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = role
                    });
            }
        }

        const string email =
            "manager@sentinelcase.local";

        var user =
            await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "SOC Manager",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result =
                await userManager.CreateAsync(
                    user,
                    "SentinelCase_2026!");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        result.Errors.Select(
                            x => x.Description)));
            }

            await userManager.AddToRoleAsync(
                user,
                "SocManager");
        }
    }
}
