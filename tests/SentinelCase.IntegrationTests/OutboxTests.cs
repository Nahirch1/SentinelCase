using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Infrastructure.Persistence;

namespace SentinelCase.IntegrationTests;

public sealed class OutboxTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OutboxTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SavingIncident_ShouldPersistCreatedEventInOutbox()
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var now = DateTimeOffset.UtcNow;

        var incident = SecurityIncident.Create(
            $"Outbox test {Guid.NewGuid()}",
            "Integration test for transactional outbox.",
            IncidentSeverity.High,
            now,
            now);

        dbContext.SecurityIncidents.Add(incident);

        await dbContext.SaveChangesAsync();

        var message =
            await dbContext.OutboxMessages
                .FirstOrDefaultAsync(x =>
                    x.Type.Contains(
                        "IncidentCreatedDomainEvent") &&
                    x.Payload.Contains(
                        incident.Id.ToString()));

        Assert.NotNull(message);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.Error);
    }
}
