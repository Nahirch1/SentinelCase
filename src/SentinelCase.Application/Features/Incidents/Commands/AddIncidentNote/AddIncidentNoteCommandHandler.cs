using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;

public sealed class AddIncidentNoteCommandHandler
    : IRequestHandler<AddIncidentNoteCommand, AddIncidentNoteResult?>
{
    private readonly ISecurityIncidentRepository _incidentRepository;
    private readonly IIncidentNoteRepository _noteRepository;
    private readonly IIncidentHistoryRepository _historyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public AddIncidentNoteCommandHandler(
        ISecurityIncidentRepository incidentRepository,
        IIncidentNoteRepository noteRepository,
        IIncidentHistoryRepository historyRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _incidentRepository = incidentRepository;
        _noteRepository = noteRepository;
        _historyRepository = historyRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<AddIncidentNoteResult?> Handle(
        AddIncidentNoteCommand request,
        CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        var createdAt = _timeProvider.GetUtcNow();

        var note = IncidentNote.Create(
            incident.Id,
            request.Content,
            _currentUser.Identifier,
            createdAt);

        await _noteRepository.AddAsync(
            note,
            cancellationToken);

        var historyEntry = IncidentHistoryEntry.Create(
            incident.Id,
            IncidentHistoryEventType.NoteAdded,
            "A note was added to the incident.",
            previousValue: null,
            newValue: note.Id.ToString(),
            _currentUser.Identifier,
            createdAt);

        await _historyRepository.AddAsync(
            historyEntry,
            cancellationToken);

        return new AddIncidentNoteResult(
            note.Id,
            note.IncidentId,
            note.Content,
            note.CreatedBy,
            note.CreatedAt);
    }
}
