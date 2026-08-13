using Microsoft.Extensions.Time.Testing;

using SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.AddIncidentNote;

public sealed class AddIncidentNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateNoteAndHistory()
    {
        var currentTime = CreateCurrentTime();

        var incidentRepository =
            new FakeSecurityIncidentRepository();

        var noteRepository =
            new FakeIncidentNoteRepository();

        var historyRepository =
            new FakeIncidentHistoryRepository();

        var incident = CreateIncident(currentTime);

        await incidentRepository.AddAsync(incident);

        var handler = new AddIncidentNoteCommandHandler(
            incidentRepository,
            noteRepository,
            historyRepository,
            new FakeCurrentUser("analyst@sentinelcase.test"),
            new FakeTimeProvider(currentTime));

        var command = new AddIncidentNoteCommand(
            incident.Id,
            "Firewall logs were reviewed and suspicious traffic was confirmed.");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(incident.Id, result.IncidentId);
        Assert.Equal(command.Content, result.Content);

        Assert.Equal(
            "analyst@sentinelcase.test",
            result.CreatedBy);

        Assert.Equal(currentTime, result.CreatedAt);

        var note = Assert.Single(noteRepository.Notes);

        Assert.Equal(result.Id, note.Id);

        var historyEntry =
            Assert.Single(historyRepository.Entries);

        Assert.Equal(
            IncidentHistoryEventType.NoteAdded,
            historyEntry.EventType);

        Assert.Equal(
            result.Id.ToString(),
            historyEntry.NewValue);

        Assert.Equal(
            "analyst@sentinelcase.test",
            historyEntry.PerformedBy);
    }

    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var currentTime = CreateCurrentTime();

        var noteRepository =
            new FakeIncidentNoteRepository();

        var historyRepository =
            new FakeIncidentHistoryRepository();

        var handler = new AddIncidentNoteCommandHandler(
            new FakeSecurityIncidentRepository(),
            noteRepository,
            historyRepository,
            new FakeCurrentUser("analyst@sentinelcase.test"),
            new FakeTimeProvider(currentTime));

        var command = new AddIncidentNoteCommand(
            Guid.NewGuid(),
            "This note should not be created.");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(noteRepository.Notes);
        Assert.Empty(historyRepository.Entries);
    }

    private static SecurityIncident CreateIncident(
        DateTimeOffset currentTime)
    {
        return SecurityIncident.Create(
            "Suspicious network activity",
            "Unexpected outbound traffic was detected.",
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
            18,
            0,
            0,
            TimeSpan.Zero);
    }
}
