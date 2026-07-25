using System.Data.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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

        builder.ConfigureTestServices(services =>
        {
            RemoveProductionDatabase(services);
            AddTestDatabase(services);
            AddTestAuthentication(services);
        });
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
