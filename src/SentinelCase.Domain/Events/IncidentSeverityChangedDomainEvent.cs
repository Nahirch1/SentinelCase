using SentinelCase.Domain.Common;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Domain.Events;

public sealed record IncidentSeverityChangedDomainEvent(
    Guid IncidentId,
    IncidentSeverity PreviousSeverity,
    IncidentSeverity NewSeverity)
    : IDomainEvent;
