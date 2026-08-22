using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Common.Interfaces;

public interface ISentinelCaseMetrics
{
    void RecordIncidentCreated(
        IncidentSeverity severity);

    void RecordIncidentStatusChanged(
        IncidentStatus previousStatus,
        IncidentStatus newStatus);
}
