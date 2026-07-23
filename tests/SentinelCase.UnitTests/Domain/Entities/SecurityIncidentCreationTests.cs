using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.UnitTests.Domain.Entities;

public sealed class SecurityIncidentCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOpenIncident()
    {
        var detectedAt = new DateTimeOffset(
            2026,
            7,
            23,
            12,
            0,
            0,
            TimeSpan.Zero);

        var createdAt = detectedAt.AddMinutes(15);

        var incident = SecurityIncident.Create(
            "Suspicious administrative login",
            "An administrative account logged in from an unknown address.",
            IncidentSeverity.High,
            detectedAt,
            createdAt);

        Assert.NotEqual(Guid.Empty, incident.Id);
        Assert.Equal("Suspicious administrative login", incident.Title);
        Assert.Equal(IncidentSeverity.High, incident.Severity);
        Assert.Equal(IncidentStatus.Open, incident.Status);
        Assert.Equal(detectedAt, incident.DetectedAt);
        Assert.Equal(createdAt, incident.CreatedAt);
        Assert.Null(incident.ClosedAt);
    }

    [Fact]
    public void Create_WithWhitespaceAroundValues_ShouldTrimThem()
    {
        var now = DateTimeOffset.UtcNow;

        var incident = SecurityIncident.Create(
            "  Malware detected  ",
            "  Antivirus detected a malicious executable.  ",
            IncidentSeverity.Critical,
            now,
            now);

        Assert.Equal("Malware detected", incident.Title);
        Assert.Equal(
            "Antivirus detected a malicious executable.",
            incident.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    public void Create_WithEmptyTitle_ShouldThrowDomainException(string title)
    {
        var now = DateTimeOffset.UtcNow;

        var exception = Assert.Throws<DomainException>(() =>
            SecurityIncident.Create(
                title,
                "Valid description",
                IncidentSeverity.Medium,
                now,
                now));

        Assert.Equal(
            "The incident title is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithDetectionDateAfterCreationDate_ShouldThrowDomainException()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var detectedAt = createdAt.AddMinutes(1);

        var exception = Assert.Throws<DomainException>(() =>
            SecurityIncident.Create(
                "Impossible date",
                "The dates are inconsistent.",
                IncidentSeverity.Low,
                detectedAt,
                createdAt));

        Assert.Equal(
            "The incident detection date cannot be later than its creation date.",
            exception.Message);
    }
}