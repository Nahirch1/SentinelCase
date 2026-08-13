using MediatR;

using SentinelCase.Application.Common.Interfaces;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentNotes;

public sealed class GetIncidentNotesQueryHandler
    : IRequestHandler<
        GetIncidentNotesQuery,
        IReadOnlyCollection<GetIncidentNoteItem>?>
{
    private readonly ISecurityIncidentRepository _incidentRepository;
    private readonly IIncidentNoteRepository _noteRepository;

    public GetIncidentNotesQueryHandler(
        ISecurityIncidentRepository incidentRepository,
        IIncidentNoteRepository noteRepository)
    {
        _incidentRepository = incidentRepository;
        _noteRepository = noteRepository;
    }

    public async Task<IReadOnlyCollection<GetIncidentNoteItem>?> Handle(
        GetIncidentNotesQuery request,
        CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        var notes = await _noteRepository.GetByIncidentIdAsync(
            request.IncidentId,
            cancellationToken);

        return notes
            .Select(note => new GetIncidentNoteItem(
                note.Id,
                note.Content,
                note.CreatedBy,
                note.CreatedAt))
            .ToArray();
    }
}
