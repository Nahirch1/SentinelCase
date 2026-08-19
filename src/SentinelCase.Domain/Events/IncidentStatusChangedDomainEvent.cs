using SentinelCase.Domain.Common;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Domain.Events;

public sealed record IncidentStatusChangedDomainEvent(
    Guid IncidentId,
    IncidentStatus PreviousStatus,
    IncidentStatus NewStatus)
    : IDomainEvent;
