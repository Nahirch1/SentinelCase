using System.Data.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SentinelCase.Infrastructure.Identity;
using SentinelCase.Infrastructure.Persistence;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "SistemaCentinela.Tests",
                    ["Jwt:Audience"] = "SistemaCentinela.Tests",
                    ["Jwt:SigningKey"] =
                        "SistemaCentinela_Test_Signing_Key_2026_AtLeast_32_Bytes"
                });
        });

        builder.ConfigureTestServices(services =>
        {
            RemoveProductionDatabase(services);
            AddTestDatabase(services);
            AddTestAuthentication(services);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        dbContext.Database.EnsureCreated();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole<Guid>>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        const string role = "SocManager";
        const string email = "manager@test.local";

        if (!roleManager.RoleExistsAsync(role)
            .GetAwaiter()
            .GetResult())
        {
            roleManager.CreateAsync(
                new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role
                })
                .GetAwaiter()
                .GetResult();
        }

        var user = userManager
            .FindByEmailAsync(email)
            .GetAwaiter()
            .GetResult();

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Test SOC Manager",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = userManager
                .CreateAsync(
                    user,
                    "TestPassword_2026!")
                .GetAwaiter()
                .GetResult();

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        result.Errors.Select(
                            x => x.Description)));
            }

            userManager.AddToRoleAsync(
                user,
                role)
                .GetAwaiter()
                .GetResult();
        }

        return host;
    }

    private static void RemoveProductionDatabase(
        IServiceCollection services)
    {
        var dbContextOptionsDescriptor =
            services.SingleOrDefault(descriptor =>
                descriptor.ServiceType ==
                typeof(
                    IDbContextOptionsConfiguration<
                        ApplicationDbContext>));

        if (dbContextOptionsDescriptor is not null)
        {
            services.Remove(dbContextOptionsDescriptor);
        }

        var dbContextDescriptor =
            services.SingleOrDefault(descriptor =>
                descriptor.ServiceType ==
                typeof(DbContextOptions<ApplicationDbContext>));

        if (dbContextDescriptor is not null)
        {
            services.Remove(dbContextDescriptor);
        }
    }

    private static void AddTestDatabase(
        IServiceCollection services)
    {
        services.AddSingleton<DbConnection>(_ =>
        {
            var connection =
                new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        });

        services.AddDbContext<ApplicationDbContext>(
            (serviceProvider, options) =>
            {
                var connection =
                    serviceProvider.GetRequiredService<DbConnection>();

                options.UseSqlite(connection);
            });
    }

    private static void AddTestAuthentication(
        IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    TestAuthHandler.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    TestAuthHandler.AuthenticationScheme;

                options.DefaultForbidScheme =
                    TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme,
                _ => { });
    }
}
