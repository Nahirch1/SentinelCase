using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Infrastructure.Persistence;
using SentinelCase.Infrastructure.Persistence.Repositories;

namespace SentinelCase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<
            ISecurityIncidentRepository,
            SecurityIncidentRepository>();

        services.AddScoped<
            IIncidentHistoryRepository,
            IncidentHistoryRepository>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
