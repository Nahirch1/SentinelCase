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

        var assignedAt = thirdCreatedAt.AddMinutes(5);

        thirdIncident.AssignTo(
            "analyst@sentinelcase.test",
            assignedAt);

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
        Assert.Equal(thirdIncident.AssignedTo, items[0].AssignedTo);
        Assert.Equal(thirdIncident.AssignedAt, items[0].AssignedAt);
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

    [Fact]
    public async Task Handle_WithAssignedToFilter_ShouldReturnOnlyAssignedIncidents()
    {
        var repository = new FakeSecurityIncidentRepository();

        var createdAt = new DateTimeOffset(
            2026,
            8,
            13,
            16,
            0,
            0,
            TimeSpan.Zero);

        var matchingIncident = SecurityIncident.Create(
            "Assigned incident",
            "Incident assigned to the requested analyst.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-20),
            createdAt);

        matchingIncident.AssignTo(
            "analyst@sentinelcase.test",
            createdAt.AddMinutes(5));

        var otherIncident = SecurityIncident.Create(
            "Other assigned incident",
            "Incident assigned to another analyst.",
            IncidentSeverity.Medium,
            createdAt.AddMinutes(-15),
            createdAt.AddMinutes(1));

        otherIncident.AssignTo(
            "other.analyst@sentinelcase.test",
            createdAt.AddMinutes(6));

        await repository.AddAsync(matchingIncident);
        await repository.AddAsync(otherIncident);

        var handler = new GetIncidentsQueryHandler(repository);

        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            AssignedTo: "analyst@sentinelcase.test");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(matchingIncident.Id, item.Id);
        Assert.Equal(
            "analyst@sentinelcase.test",
            item.AssignedTo);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithSeverityFilter_ShouldReturnOnlyMatchingIncidents()
    {
        var repository = new FakeSecurityIncidentRepository();
        var createdAt = DateTimeOffset.UtcNow;

        var lowIncident = SecurityIncident.Create(
            "Low severity incident",
            "Low severity description.",
            IncidentSeverity.Low,
            createdAt.AddMinutes(-10),
            createdAt);

        var highIncident = SecurityIncident.Create(
            "High severity incident",
            "High severity description.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(1));

        await repository.AddAsync(lowIncident);
        await repository.AddAsync(highIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            Severity: IncidentSeverity.High);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(highIncident.Id, item.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnOnlyMatchingIncidents()
    {
        var repository = new FakeSecurityIncidentRepository();
        var createdAt = DateTimeOffset.UtcNow;

        var openIncident = SecurityIncident.Create(
            "Open incident",
            "Open incident description.",
            IncidentSeverity.Medium,
            createdAt.AddMinutes(-10),
            createdAt);

        var investigationIncident = SecurityIncident.Create(
            "Incident under investigation",
            "Investigation description.",
            IncidentSeverity.Medium,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(1));

        investigationIncident.StartInvestigation();

        await repository.AddAsync(openIncident);
        await repository.AddAsync(investigationIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            Status: IncidentStatus.UnderInvestigation);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(investigationIncident.Id, item.Id);
        Assert.Equal(IncidentStatus.UnderInvestigation, item.Status);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithSearchTermInTitle_ShouldReturnMatchingIncident()
    {
        var repository = new FakeSecurityIncidentRepository();
        var createdAt = DateTimeOffset.UtcNow;

        var matchingIncident = SecurityIncident.Create(
            "Suspicious PowerShell execution",
            "Command execution detected.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-10),
            createdAt);

        var otherIncident = SecurityIncident.Create(
            "Failed login attempts",
            "Several failed authentication attempts.",
            IncidentSeverity.Medium,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(1));

        await repository.AddAsync(matchingIncident);
        await repository.AddAsync(otherIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            SearchTerm: "powershell");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(matchingIncident.Id, item.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithSearchTermInDescription_ShouldReturnMatchingIncident()
    {
        var repository = new FakeSecurityIncidentRepository();
        var createdAt = DateTimeOffset.UtcNow;

        var matchingIncident = SecurityIncident.Create(
            "Suspicious process",
            "Malware beacon traffic was detected.",
            IncidentSeverity.Critical,
            createdAt.AddMinutes(-10),
            createdAt);

        var otherIncident = SecurityIncident.Create(
            "Account lockout",
            "User account was automatically locked.",
            IncidentSeverity.Low,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(1));

        await repository.AddAsync(matchingIncident);
        await repository.AddAsync(otherIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            SearchTerm: "beacon");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(matchingIncident.Id, item.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithCombinedFilters_ShouldReturnOnlyMatchingIncident()
    {
        var repository = new FakeSecurityIncidentRepository();
        var createdAt = DateTimeOffset.UtcNow;

        var matchingIncident = SecurityIncident.Create(
            "Suspicious PowerShell execution",
            "PowerShell command detected on endpoint.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-10),
            createdAt);

        var wrongSeverityIncident = SecurityIncident.Create(
            "PowerShell policy warning",
            "PowerShell configuration warning.",
            IncidentSeverity.Low,
            createdAt.AddMinutes(-5),
            createdAt.AddMinutes(1));

        var wrongStatusIncident = SecurityIncident.Create(
            "PowerShell investigation",
            "PowerShell activity under investigation.",
            IncidentSeverity.High,
            createdAt,
            createdAt.AddMinutes(2));

        wrongStatusIncident.StartInvestigation();

        await repository.AddAsync(matchingIncident);
        await repository.AddAsync(wrongSeverityIncident);
        await repository.AddAsync(wrongStatusIncident);

        var handler = new GetIncidentsQueryHandler(repository);
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20,
            Status: IncidentStatus.Open,
            Severity: IncidentSeverity.High,
            SearchTerm: "PowerShell");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(matchingIncident.Id, item.Id);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

}
