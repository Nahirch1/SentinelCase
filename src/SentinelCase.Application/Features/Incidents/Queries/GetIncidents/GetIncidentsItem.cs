using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

public sealed record GetIncidentsItem(
    Guid Id,
    string Title,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTimeOffset DetectedAt,
    DateTimeOffset CreatedAt,
    string? AssignedTo,
    DateTimeOffset? AssignedAt);
