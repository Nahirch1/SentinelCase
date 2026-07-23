using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Domain.Entities;

public sealed class SecurityIncident
{
    private const int MaximumTitleLength = 200;
    private const int MaximumDescriptionLength = 4000;

    private SecurityIncident()
    {
    }

    private SecurityIncident(
        Guid id,
        string title,
        string description,
        IncidentSeverity severity,
        DateTimeOffset detectedAt,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Severity = severity;
        Status = IncidentStatus.Open;
        DetectedAt = detectedAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IncidentSeverity Severity { get; private set; }

    public IncidentStatus Status { get; private set; }

    public DateTimeOffset DetectedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public static SecurityIncident Create(
        string title,
        string description,
        IncidentSeverity severity,
        DateTimeOffset detectedAt,
        DateTimeOffset createdAt)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        ValidateSeverity(severity);

        if (detectedAt > createdAt)
        {
            throw new DomainException(
                "The incident detection date cannot be later than its creation date.");
        }

        return new SecurityIncident(
            Guid.NewGuid(),
            title.Trim(),
            description.Trim(),
            severity,
            detectedAt,
            createdAt);
    }

    public void StartInvestigation()
    {
        EnsureNotClosed();

        if (Status != IncidentStatus.Open)
        {
            throw new DomainException(
                "Only an open incident can enter investigation.");
        }

        Status = IncidentStatus.UnderInvestigation;
    }

    public void Contain()
    {
        EnsureNotClosed();

        if (Status != IncidentStatus.UnderInvestigation)
        {
            throw new DomainException(
                "Only an incident under investigation can be contained.");
        }

        Status = IncidentStatus.Contained;
    }

    public void Resolve()
    {
        EnsureNotClosed();

        if (Status != IncidentStatus.Contained)
        {
            throw new DomainException(
                "Only a contained incident can be resolved.");
        }

        Status = IncidentStatus.Resolved;
    }

    public void Close(DateTimeOffset closedAt)
    {
        EnsureNotClosed();

        if (Status != IncidentStatus.Resolved)
        {
            throw new DomainException(
                "Only a resolved incident can be closed.");
        }

        if (closedAt < CreatedAt)
        {
            throw new DomainException(
                "The closure date cannot be earlier than the creation date.");
        }

        Status = IncidentStatus.Closed;
        ClosedAt = closedAt;
    }

    public void ChangeSeverity(IncidentSeverity severity)
    {
        EnsureNotClosed();
        ValidateSeverity(severity);

        Severity = severity;
    }

    public void UpdateDetails(string title, string description)
    {
        EnsureNotClosed();
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = description.Trim();
    }

    private void EnsureNotClosed()
    {
        if (Status == IncidentStatus.Closed)
        {
            throw new DomainException(
                "A closed incident cannot be modified.");
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException(
                "The incident title is required.");
        }

        if (title.Trim().Length > MaximumTitleLength)
        {
            throw new DomainException(
                $"The incident title cannot exceed {MaximumTitleLength} characters.");
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException(
                "The incident description is required.");
        }

        if (description.Trim().Length > MaximumDescriptionLength)
        {
            throw new DomainException(
                $"The incident description cannot exceed {MaximumDescriptionLength} characters.");
        }
    }

    private static void ValidateSeverity(IncidentSeverity severity)
    {
        if (!Enum.IsDefined(severity))
        {
            throw new DomainException(
                "The incident severity is invalid.");
        }
    }
}
