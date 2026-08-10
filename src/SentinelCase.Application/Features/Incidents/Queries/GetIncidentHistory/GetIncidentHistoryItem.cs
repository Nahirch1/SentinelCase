using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;

public sealed record GetIncidentHistoryItem(
    Guid Id,
    IncidentHistoryEventType EventType,
    string Description,
    string? PreviousValue,
    string? NewValue,
    string PerformedBy,
    DateTimeOffset OccurredAt);
