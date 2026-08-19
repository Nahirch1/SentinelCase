using SentinelCase.Domain.Common;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Domain.Events;

public sealed record IncidentCreatedDomainEvent(
    Guid IncidentId,
    string Title,
    IncidentSeverity Severity)
    : IDomainEvent;
