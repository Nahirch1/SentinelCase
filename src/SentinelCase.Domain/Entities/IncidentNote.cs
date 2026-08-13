using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Domain.Entities;

public sealed class IncidentNote
{
    private const int MaximumContentLength = 4000;
    private const int MaximumAuthorLength = 200;

    private IncidentNote()
    {
    }

    private IncidentNote(
        Guid id,
        Guid incidentId,
        string content,
        string createdBy,
        DateTimeOffset createdAt)
    {
        Id = id;
        IncidentId = incidentId;
        Content = content;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid IncidentId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static IncidentNote Create(
        Guid incidentId,
        string content,
        string createdBy,
        DateTimeOffset createdAt)
    {
        if (incidentId == Guid.Empty)
        {
            throw new DomainException(
                "The incident identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException(
                "The note content is required.");
        }

        if (content.Trim().Length > MaximumContentLength)
        {
            throw new DomainException(
                $"The note content cannot exceed {MaximumContentLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new DomainException(
                "The note author is required.");
        }

        if (createdBy.Trim().Length > MaximumAuthorLength)
        {
            throw new DomainException(
                $"The note author cannot exceed {MaximumAuthorLength} characters.");
        }

        return new IncidentNote(
            Guid.NewGuid(),
            incidentId,
            content.Trim(),
            createdBy.Trim(),
            createdAt);
    }
}
