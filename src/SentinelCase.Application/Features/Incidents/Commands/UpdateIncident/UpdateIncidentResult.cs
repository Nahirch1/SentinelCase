using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;

public sealed record UpdateIncidentResult(
    Guid Id,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status);
