using SentinelCase.Application.Features.Incidents.Queries.GetIncidents;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Queries.GetIncidents;

public sealed class GetIncidentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithIncidents_ShouldReturnPagedResult()
    {
        var repository = new FakeSecurityIncidentRepository();

        var firstCreatedAt = new DateTimeOffset(
            2026,
            7,
            23,
            18,
            0,
            0,
            TimeSpan.Zero);

        var secondCreatedAt = firstCreatedAt.AddMinutes(10);
        var thirdCreatedAt = firstCreatedAt.AddMinutes(20);

        var firstIncident = SecurityIncident.Create(
            "First incident",
            "First incident description.",
            IncidentSeverity.Low,
            firstCreatedAt.AddMinutes(-10),
            firstCreatedAt);

        var secondIncident = SecurityIncident.Create(
            "Second incident",
            "Second incident description.",
            IncidentSeverity.Medium,
            secondCreatedAt.AddMinutes(-10),
            secondCreatedAt);

        var thirdIncident = SecurityIncident.Create(
            "Third incident",
            "Third incident description.",
            IncidentSeverity.High,
            thirdCreatedAt.AddMinutes(-10),
            thirdCreatedAt);

        await repository.AddAsync(firstIncident);
        await repository.AddAsync(secondIncident);
        await repository.AddAsync(thirdIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 2);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);

        Assert.Equal(2, result.Items.Count);

        var items = result.Items.ToArray();

        Assert.Equal(thirdIncident.Id, items[0].Id);
        Assert.Equal(secondIncident.Id, items[1].Id);

        Assert.Equal(thirdIncident.Title, items[0].Title);
        Assert.Equal(thirdIncident.Severity, items[0].Severity);
        Assert.Equal(thirdIncident.Status, items[0].Status);
        Assert.Equal(thirdIncident.DetectedAt, items[0].DetectedAt);
        Assert.Equal(thirdIncident.CreatedAt, items[0].CreatedAt);
    }

    [Fact]
    public async Task Handle_WithSecondPage_ShouldReturnRemainingIncident()
    {
        var repository = new FakeSecurityIncidentRepository();

        var createdAt = new DateTimeOffset(
            2026,
            7,
            23,
            18,
            0,
            0,
            TimeSpan.Zero);

        for (var index = 1; index <= 3; index++)
        {
            var incident = SecurityIncident.Create(
                $"Incident {index}",
                $"Description for incident {index}.",
                IncidentSeverity.Medium,
                createdAt.AddMinutes(index - 1),
                createdAt.AddMinutes(index));

            await repository.AddAsync(incident);
        }

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 2,
            PageSize: 2);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        Assert.Single(result.Items);
    }
}
