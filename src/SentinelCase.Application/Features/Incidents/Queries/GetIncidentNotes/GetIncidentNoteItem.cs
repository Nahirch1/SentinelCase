namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentNotes;

public sealed record GetIncidentNoteItem(
    Guid Id,
    string Content,
    string CreatedBy,
    DateTimeOffset CreatedAt);
