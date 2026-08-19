using SentinelCase.Domain.Common;

namespace SentinelCase.Domain.Events;

public sealed record IncidentDetailsUpdatedDomainEvent(
    Guid IncidentId)
    : IDomainEvent;
