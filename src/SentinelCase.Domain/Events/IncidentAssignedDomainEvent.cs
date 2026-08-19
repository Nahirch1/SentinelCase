using SentinelCase.Domain.Common;

namespace SentinelCase.Domain.Events;

public sealed record IncidentAssignedDomainEvent(
    Guid IncidentId,
    string AnalystIdentifier)
    : IDomainEvent;
