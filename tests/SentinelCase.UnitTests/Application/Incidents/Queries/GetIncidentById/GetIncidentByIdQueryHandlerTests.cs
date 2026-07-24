using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Queries.GetIncidentById;

public sealed class GetIncidentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingIncident_ShouldReturnIncident()
    {
        var createdAt = new DateTimeOffset(
            2026,
            7,
            23,
            18,
            0,
            0,
            TimeSpan.Zero);

        var repository = new FakeSecurityIncidentRepository();

        var incident = SecurityIncident.Create(
            "Suspicious PowerShell execution",
            "Encoded PowerShell command detected on workstation FIN-023.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-30),
            createdAt);

        await repository.AddAsync(incident);

        var handler = new GetIncidentByIdQueryHandler(repository);
        var query = new GetIncidentByIdQuery(incident.Id);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(incident.Id, result.Id);
        Assert.Equal(incident.Title, result.Title);
        Assert.Equal(incident.Description, result.Description);
        Assert.Equal(incident.Severity, result.Severity);
        Assert.Equal(incident.Status, result.Status);
        Assert.Equal(incident.DetectedAt, result.DetectedAt);
        Assert.Equal(incident.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var repository = new FakeSecurityIncidentRepository();
        var handler = new GetIncidentByIdQueryHandler(repository);
        var query = new GetIncidentByIdQuery(Guid.NewGuid());

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Null(result);
    }
}
