using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;

public sealed record GetIncidentByIdResult(
    Guid Id,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTimeOffset DetectedAt,
    DateTimeOffset CreatedAt,
    string? AssignedTo,
    DateTimeOffset? AssignedAt);
