using Microsoft.Extensions.Time.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateIncidentAndHistoryEntry()
    {
        var currentTime = new DateTimeOffset(
            2026,
            7,
            23,
            18,
            0,
            0,
            TimeSpan.Zero);

        var timeProvider = new FakeTimeProvider(currentTime);
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();
        var currentUser = new FakeCurrentUser(
            "analyst@sentinelcase.test");

        var handler = new CreateIncidentCommandHandler(
            repository,
            historyRepository,
            currentUser,
            timeProvider);

        var command = new CreateIncidentCommand(
            "Suspicious privileged login",
            "A privileged account logged in from an unknown address.",
            IncidentSeverity.High,
            currentTime.AddMinutes(-20));

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(IncidentSeverity.High, result.Severity);
        Assert.Equal(IncidentStatus.Open, result.Status);
        Assert.Equal(command.DetectedAt, result.DetectedAt);
        Assert.Equal(currentTime, result.CreatedAt);

        var storedIncident = Assert.Single(repository.Incidents);

        Assert.Equal(result.Id, storedIncident.Id);
        Assert.Equal(currentTime, storedIncident.CreatedAt);

        var historyEntry = Assert.Single(historyRepository.Entries);

        Assert.NotEqual(Guid.Empty, historyEntry.Id);
        Assert.Equal(result.Id, historyEntry.IncidentId);
        Assert.Equal(
            IncidentHistoryEventType.Created,
            historyEntry.EventType);
        Assert.Equal(
            "The incident was created.",
            historyEntry.Description);
        Assert.Null(historyEntry.PreviousValue);
        Assert.Equal(
            IncidentStatus.Open.ToString(),
            historyEntry.NewValue);
        Assert.Equal(
            "analyst@sentinelcase.test",
            historyEntry.PerformedBy);
        Assert.Equal(currentTime, historyEntry.OccurredAt);
    }

    [Fact]
    public async Task Handle_WithDuplicatedTitle_ShouldThrowDomainException()
    {
        var currentTime = new DateTimeOffset(
            2026,
            7,
            23,
            18,
            0,
            0,
            TimeSpan.Zero);

        var timeProvider = new FakeTimeProvider(currentTime);
        var repository = new FakeSecurityIncidentRepository();
        var historyRepository = new FakeIncidentHistoryRepository();
        var currentUser = new FakeCurrentUser(
            "analyst@sentinelcase.test");

        var existingIncident = SecurityIncident.Create(
            "Malware detected",
            "A malicious executable was detected.",
            IncidentSeverity.Critical,
            currentTime.AddMinutes(-30),
            currentTime.AddMinutes(-25));

        await repository.AddAsync(existingIncident);

        var handler = new CreateIncidentCommandHandler(
            repository,
            historyRepository,
            currentUser,
            timeProvider);

        var command = new CreateIncidentCommand(
            "malware detected",
            "A second incident with the same title.",
            IncidentSeverity.High,
            currentTime.AddMinutes(-10));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(
            "An incident with the same title already exists.",
            exception.Message);

        Assert.Single(repository.Incidents);
        Assert.Empty(historyRepository.Entries);
    }
}
