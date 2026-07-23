using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentResult(
    Guid Id,
    string Title,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTimeOffset DetectedAt,
    DateTimeOffset CreatedAt);