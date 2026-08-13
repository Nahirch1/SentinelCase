namespace SentinelCase.Application.Features.Incidents.Commands.AssignIncident;

public sealed record AssignIncidentResult(
    Guid Id,
    string AssignedTo,
    DateTimeOffset AssignedAt);
