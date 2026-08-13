using SentinelCase.Application.Features.Incidents.Queries.GetIncidentNotes;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Queries.GetIncidentNotes;

public sealed class GetIncidentNotesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingIncident_ShouldReturnOrderedNotes()
    {
        var currentTime = new DateTimeOffset(
            2026,
            8,
            13,
            18,
            0,
            0,
            TimeSpan.Zero);

        var incidentRepository =
            new FakeSecurityIncidentRepository();

        var noteRepository =
            new FakeIncidentNoteRepository();

        var incident = SecurityIncident.Create(
            "Suspicious network activity",
            "Unexpected outbound traffic was detected.",
            IncidentSeverity.High,
            currentTime.AddMinutes(-30),
            currentTime.AddMinutes(-20));

        await incidentRepository.AddAsync(incident);

        var firstNote = IncidentNote.Create(
            incident.Id,
            "First investigation note.",
            "analyst1@sentinelcase.test",
            currentTime.AddMinutes(-5));

        var secondNote = IncidentNote.Create(
            incident.Id,
            "Second investigation note.",
            "analyst2@sentinelcase.test",
            currentTime);

        await noteRepository.AddAsync(secondNote);
        await noteRepository.AddAsync(firstNote);

        var handler = new GetIncidentNotesQueryHandler(
            incidentRepository,
            noteRepository);

        var query = new GetIncidentNotesQuery(
            incident.Id);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var notes = result.ToArray();

        Assert.Equal(firstNote.Id, notes[0].Id);
        Assert.Equal(firstNote.Content, notes[0].Content);
        Assert.Equal(firstNote.CreatedBy, notes[0].CreatedBy);
        Assert.Equal(firstNote.CreatedAt, notes[0].CreatedAt);

        Assert.Equal(secondNote.Id, notes[1].Id);
        Assert.Equal(secondNote.Content, notes[1].Content);
    }

    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var handler = new GetIncidentNotesQueryHandler(
            new FakeSecurityIncidentRepository(),
            new FakeIncidentNoteRepository());

        var query = new GetIncidentNotesQuery(
            Guid.NewGuid());

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Null(result);
    }
}
