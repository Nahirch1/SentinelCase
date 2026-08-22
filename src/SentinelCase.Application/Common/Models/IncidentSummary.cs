namespace SentinelCase.Application.Common.Models;

public sealed record IncidentSummary(
    int Total,
    int Open,
    int Critical,
    int UnderInvestigation,
    int Contained,
    int Resolved,
    int Closed,
    int LowSeverity,
    int MediumSeverity,
    int HighSeverity,
    int CriticalSeverity);
