namespace SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;

public sealed record AddIncidentNoteResult(
    Guid Id,
    Guid IncidentId,
    string Content,
    string CreatedBy,
    DateTimeOffset CreatedAt);
