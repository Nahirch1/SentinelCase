using Microsoft.Extensions.Time.Testing;

using SentinelCase.Application.Features.Incidents.Commands.AssignIncident;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.AssignIncident;

public sealed class AssignIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldAssignIncidentAndCreateHistory()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();
        var currentUser = new FakeCurrentUser(
            "manager@sentinelcase.test");

        var incident = CreateIncident(currentTime);

        await repository.AddAsync(incident);

        var handler = new AssignIncidentCommandHandler(
            repository,
            historyRepository,
            currentUser,
            new FakeTimeProvider(currentTime));

        var command = new AssignIncidentCommand(
            incident.Id,
            "analyst@sentinelcase.test");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(incident.Id, result.Id);
        Assert.Equal(
            "analyst@sentinelcase.test",
            result.AssignedTo);
        Assert.Equal(currentTime, result.AssignedAt);

        Assert.Equal(
            "analyst@sentinelcase.test",
            incident.AssignedTo);
        Assert.Equal(currentTime, incident.AssignedAt);

        var historyEntry =
            Assert.Single(historyRepository.Entries);

        Assert.Equal(
            IncidentHistoryEventType.Assigned,
            historyEntry.EventType);

        Assert.Null(historyEntry.PreviousValue);

        Assert.Equal(
            "analyst@sentinelcase.test",
            historyEntry.NewValue);

        Assert.Equal(
            "manager@sentinelcase.test",
            historyEntry.PerformedBy);

        Assert.Equal(
            currentTime,
            historyEntry.OccurredAt);
    }

    [Fact]
    public async Task Handle_WhenReassigned_ShouldRecordPreviousAnalyst()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();

        var incident = CreateIncident(currentTime);

        incident.AssignTo(
            "first.analyst@sentinelcase.test",
            currentTime.AddMinutes(-10));

        await repository.AddAsync(incident);

        var handler = new AssignIncidentCommandHandler(
            repository,
            historyRepository,
            new FakeCurrentUser("manager@sentinelcase.test"),
            new FakeTimeProvider(currentTime));

        var command = new AssignIncidentCommand(
            incident.Id,
            "second.analyst@sentinelcase.test");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);

        Assert.Equal(
            "second.analyst@sentinelcase.test",
            result.AssignedTo);

        var historyEntry =
            Assert.Single(historyRepository.Entries);

        Assert.Equal(
            "first.analyst@sentinelcase.test",
            historyEntry.PreviousValue);

        Assert.Equal(
            "second.analyst@sentinelcase.test",
            historyEntry.NewValue);
    }

    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var currentTime = CreateCurrentTime();

        var handler = new AssignIncidentCommandHandler(
            new FakeSecurityIncidentRepository(),
            new FakeIncidentHistoryRepository(),
            new FakeCurrentUser("manager@sentinelcase.test"),
            new FakeTimeProvider(currentTime));

        var command = new AssignIncidentCommand(
            Guid.NewGuid(),
            "analyst@sentinelcase.test");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithClosedIncident_ShouldThrowDomainException()
    {
        var currentTime = CreateCurrentTime();
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();

        var incident = CreateIncident(currentTime);

        incident.StartInvestigation();
        incident.Contain();
        incident.Resolve();
        incident.Close(currentTime);

        await repository.AddAsync(incident);

        var handler = new AssignIncidentCommandHandler(
            repository,
            historyRepository,
            new FakeCurrentUser("manager@sentinelcase.test"),
            new FakeTimeProvider(currentTime));

        var command = new AssignIncidentCommand(
            incident.Id,
            "analyst@sentinelcase.test");

        var exception =
            await Assert.ThrowsAsync<DomainException>(() =>
                handler.Handle(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "A closed incident cannot be modified.",
            exception.Message);

        Assert.Empty(historyRepository.Entries);
    }

    private static SecurityIncident CreateIncident(
        DateTimeOffset currentTime)
    {
        return SecurityIncident.Create(
            "Suspicious authentication activity",
            "Repeated authentication failures were detected.",
            IncidentSeverity.High,
            currentTime.AddMinutes(-30),
            currentTime.AddMinutes(-20));
    }

    private static DateTimeOffset CreateCurrentTime()
    {
        return new DateTimeOffset(
            2026,
            8,
            13,
            16,
            0,
            0,
            TimeSpan.Zero);
    }
}
