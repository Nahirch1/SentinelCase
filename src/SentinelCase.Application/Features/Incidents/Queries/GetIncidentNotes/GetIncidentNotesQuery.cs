using MediatR;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentNotes;

public sealed record GetIncidentNotesQuery(
    Guid IncidentId)
    : IRequest<IReadOnlyCollection<GetIncidentNoteItem>?>;
