using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Enums;

namespace SentinelCase.UnitTests.TestDoubles;

internal sealed class FakeSentinelCaseMetrics
    : ISentinelCaseMetrics
{
    public int IncidentsCreated { get; private set; }

    public int StatusChanges { get; private set; }

    public void RecordIncidentCreated(
        IncidentSeverity severity)
    {
        IncidentsCreated++;
    }

    public void RecordIncidentStatusChanged(
        IncidentStatus previousStatus,
        IncidentStatus newStatus)
    {
        StatusChanges++;
    }
}
