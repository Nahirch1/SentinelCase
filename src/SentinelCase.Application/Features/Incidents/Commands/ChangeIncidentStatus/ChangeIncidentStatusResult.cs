using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public sealed record ChangeIncidentStatusResult(
    Guid Id,
    IncidentStatus Status,
    DateTimeOffset? ClosedAt);
