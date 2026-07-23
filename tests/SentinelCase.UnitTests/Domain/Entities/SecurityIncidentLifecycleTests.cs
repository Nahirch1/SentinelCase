using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.UnitTests.Domain.Entities;

public sealed class SecurityIncidentLifecycleTests
{
    [Fact]
    public void CompleteLifecycle_ShouldFinishWithClosedStatus()
    {
        var now = DateTimeOffset.UtcNow;

        var incident = SecurityIncident.Create(
            "Compromised workstation",
            "A workstation presented indicators of compromise.",
            IncidentSeverity.High,
            now,
            now);

        incident.StartInvestigation();
        Assert.Equal(IncidentStatus.UnderInvestigation, incident.Status);

        incident.Contain();
        Assert.Equal(IncidentStatus.Contained, incident.Status);

        incident.Resolve();
        Assert.Equal(IncidentStatus.Resolved, incident.Status);

        incident.Close(now.AddHours(2));

        Assert.Equal(IncidentStatus.Closed, incident.Status);
        Assert.Equal(now.AddHours(2), incident.ClosedAt);
    }

    [Fact]
    public void Close_OpenIncident_ShouldThrowDomainException()
    {
        var now = DateTimeOffset.UtcNow;

        var incident = SecurityIncident.Create(
            "Unauthorized access",
            "An unauthorized access attempt was registered.",
            IncidentSeverity.Medium,
            now,
            now);

        var exception = Assert.Throws<DomainException>(() =>
            incident.Close(now.AddHours(1)));

        Assert.Equal(
            "Only a resolved incident can be closed.",
            exception.Message);
    }

    [Fact]
    public void ChangeSeverity_OnClosedIncident_ShouldThrowDomainException()
    {
        var now = DateTimeOffset.UtcNow;

        var incident = SecurityIncident.Create(
            "Resolved incident",
            "Incident used to validate closed-state behavior.",
            IncidentSeverity.Low,
            now,
            now);

        incident.StartInvestigation();
        incident.Contain();
        incident.Resolve();
        incident.Close(now.AddMinutes(30));

        var exception = Assert.Throws<DomainException>(() =>
            incident.ChangeSeverity(IncidentSeverity.Critical));

        Assert.Equal(
            "A closed incident cannot be modified.",
            exception.Message);
    }
}
