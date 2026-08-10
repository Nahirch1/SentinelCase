using Microsoft.Extensions.Time.Testing;

using SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.ChangeIncidentStatus;

public sealed class ChangeIncidentStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            Guid.NewGuid(),
            IncidentStatus.UnderInvestigation);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_FromOpenToUnderInvestigation_ShouldChangeStatus()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.UnderInvestigation);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(
            IncidentStatus.UnderInvestigation,
            result.Status);
        Assert.Equal(
            IncidentStatus.UnderInvestigation,
            incident.Status);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task Handle_FromUnderInvestigationToContained_ShouldChangeStatus()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Contained);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(IncidentStatus.Contained, result.Status);
        Assert.Equal(IncidentStatus.Contained, incident.Status);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task Handle_FromContainedToResolved_ShouldChangeStatus()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();
        incident.Contain();

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Resolved);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(IncidentStatus.Resolved, result.Status);
        Assert.Equal(IncidentStatus.Resolved, incident.Status);
        Assert.Null(result.ClosedAt);
    }

    [Fact]
    public async Task Handle_FromResolvedToClosed_ShouldSetClosedAt()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();
        incident.Contain();
        incident.Resolve();

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Closed);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(IncidentStatus.Closed, result.Status);
        Assert.Equal(IncidentStatus.Closed, incident.Status);
        Assert.Equal(currentTime, result.ClosedAt);
        Assert.Equal(currentTime, incident.ClosedAt);
    }

    [Fact]
    public async Task Handle_WithSameStatus_ShouldReturnCurrentIncident()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Open);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(IncidentStatus.Open, result.Status);
        Assert.Null(result.ClosedAt);
        Assert.Equal(IncidentStatus.Open, incident.Status);
    }

    [Fact]
    public async Task Handle_WithInvalidTransition_ShouldThrowDomainException()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Contained);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(IncidentStatus.Open, incident.Status);
    }

    [Fact]
    public async Task Handle_WhenReturningToOpen_ShouldThrowDomainException()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Open);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(
            "An incident cannot return to the open status.",
            exception.Message);

        Assert.Equal(
            IncidentStatus.UnderInvestigation,
            incident.Status);
    }

    [Fact]
    public async Task Handle_WhenStatusChanges_ShouldAddStatusChangedHistory()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();
        var incident = CreateIncident(currentTime);

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime,
            historyRepository);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.UnderInvestigation);

        await handler.Handle(
            command,
            CancellationToken.None);

        var historyEntry = Assert.Single(historyRepository.Entries);

        Assert.Equal(
            IncidentHistoryEventType.StatusChanged,
            historyEntry.EventType);

        Assert.Equal(
            IncidentStatus.Open.ToString(),
            historyEntry.PreviousValue);

        Assert.Equal(
            IncidentStatus.UnderInvestigation.ToString(),
            historyEntry.NewValue);

        Assert.Equal(
            "analyst@sentinelcase.test",
            historyEntry.PerformedBy);

        Assert.Equal(
            currentTime,
            historyEntry.OccurredAt);
    }

    [Fact]
    public async Task Handle_WhenIncidentCloses_ShouldAddClosedHistory()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();
        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();
        incident.Contain();
        incident.Resolve();

        await repository.AddAsync(incident);

        var handler = CreateHandler(
            repository,
            currentTime,
            historyRepository);

        var command = new ChangeIncidentStatusCommand(
            incident.Id,
            IncidentStatus.Closed);

        await handler.Handle(
            command,
            CancellationToken.None);

        var historyEntry = Assert.Single(historyRepository.Entries);

        Assert.Equal(
            IncidentHistoryEventType.Closed,
            historyEntry.EventType);

        Assert.Equal(
            IncidentStatus.Resolved.ToString(),
            historyEntry.PreviousValue);

        Assert.Equal(
            IncidentStatus.Closed.ToString(),
            historyEntry.NewValue);
    }

    private static ChangeIncidentStatusCommandHandler CreateHandler(
        FakeSecurityIncidentRepository repository,
        DateTimeOffset currentTime,
        FakeIncidentHistoryRepository? historyRepository = null)
    {
        return new ChangeIncidentStatusCommandHandler(
            repository,
            historyRepository ?? new FakeIncidentHistoryRepository(),
            new FakeCurrentUser("analyst@sentinelcase.test"),
            new FakeTimeProvider(currentTime));
    }

    private static SecurityIncident CreateIncident(
        DateTimeOffset currentTime)
    {
        return SecurityIncident.Create(
            "Suspicious PowerShell activity",
            "An encoded PowerShell command was detected.",
            IncidentSeverity.High,
            currentTime.AddMinutes(-30),
            currentTime.AddMinutes(-25));
    }

    private static DateTimeOffset CreateCurrentTime()
    {
        return new DateTimeOffset(
            2026,
            7,
            24,
            20,
            0,
            0,
            TimeSpan.Zero);
    }
}
