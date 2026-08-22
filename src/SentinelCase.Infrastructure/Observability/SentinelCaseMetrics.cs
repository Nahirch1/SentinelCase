using System.Diagnostics.Metrics;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Infrastructure.Observability;

public sealed class SentinelCaseMetrics
    : ISentinelCaseMetrics
{
    public const string MeterName =
        "SentinelCase";

    private readonly Meter _meter;

    private readonly Counter<long> _outboxProcessed;
    private readonly Counter<long> _outboxFailed;
    private readonly Histogram<double> _outboxProcessingDuration;

    private readonly Counter<long> _incidentsCreated;
    private readonly Counter<long> _incidentStatusChanged;

    public SentinelCaseMetrics()
    {
        _meter = new Meter(
            MeterName,
            "1.0.0");

        _outboxProcessed =
            _meter.CreateCounter<long>(
                "sentinelcase.outbox.processed",
                unit: "{message}",
                description:
                    "Number of successfully processed outbox messages.");

        _outboxFailed =
            _meter.CreateCounter<long>(
                "sentinelcase.outbox.failed",
                unit: "{message}",
                description:
                    "Number of failed outbox message processing attempts.");

        _outboxProcessingDuration =
            _meter.CreateHistogram<double>(
                "sentinelcase.outbox.processing.duration",
                unit: "ms",
                description:
                    "Outbox message processing duration.");

        _incidentsCreated =
            _meter.CreateCounter<long>(
                "sentinelcase.incidents.created",
                unit: "{incident}",
                description:
                    "Number of incidents created.");

        _incidentStatusChanged =
            _meter.CreateCounter<long>(
                "sentinelcase.incidents.status_changed",
                unit: "{change}",
                description:
                    "Number of incident status changes.");
    }

    public void RecordOutboxProcessed(
        string messageType,
        double durationMilliseconds)
    {
        _outboxProcessed.Add(
            1,
            new KeyValuePair<string, object?>(
                "message.type",
                messageType));

        _outboxProcessingDuration.Record(
            durationMilliseconds,
            new KeyValuePair<string, object?>(
                "message.type",
                messageType));
    }

    public void RecordOutboxFailure(
        string messageType)
    {
        _outboxFailed.Add(
            1,
            new KeyValuePair<string, object?>(
                "message.type",
                messageType));
    }

    public void RecordIncidentCreated(
        IncidentSeverity severity)
    {
        _incidentsCreated.Add(
            1,
            new KeyValuePair<string, object?>(
                "incident.severity",
                severity.ToString()));
    }

    public void RecordIncidentStatusChanged(
        IncidentStatus previousStatus,
        IncidentStatus newStatus)
    {
        _incidentStatusChanged.Add(
            1,
            new KeyValuePair<string, object?>(
                "incident.previous_status",
                previousStatus.ToString()),
            new KeyValuePair<string, object?>(
                "incident.new_status",
                newStatus.ToString()));
    }
}
