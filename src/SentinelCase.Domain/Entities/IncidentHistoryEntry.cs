using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Domain.Entities;

public sealed class IncidentHistoryEntry
{
    private const int MaximumDescriptionLength = 1000;
    private const int MaximumValueLength = 500;
    private const int MaximumActorLength = 200;

    private IncidentHistoryEntry()
    {
    }

    private IncidentHistoryEntry(
        Guid id,
        Guid incidentId,
        IncidentHistoryEventType eventType,
        string description,
        string? previousValue,
        string? newValue,
        string performedBy,
        DateTimeOffset occurredAt)
    {
        Id = id;
        IncidentId = incidentId;
        EventType = eventType;
        Description = description;
        PreviousValue = previousValue;
        NewValue = newValue;
        PerformedBy = performedBy;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid IncidentId { get; private set; }

    public IncidentHistoryEventType EventType { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? PreviousValue { get; private set; }

    public string? NewValue { get; private set; }

    public string PerformedBy { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public static IncidentHistoryEntry Create(
        Guid incidentId,
        IncidentHistoryEventType eventType,
        string description,
        string? previousValue,
        string? newValue,
        string performedBy,
        DateTimeOffset occurredAt)
    {
        if (incidentId == Guid.Empty)
        {
            throw new DomainException(
                "The incident identifier is required.");
        }

        if (!Enum.IsDefined(eventType))
        {
            throw new DomainException(
                "The history event type is invalid.");
        }

        ValidateRequiredText(
            description,
            MaximumDescriptionLength,
            "The history description");

        ValidateOptionalText(
            previousValue,
            MaximumValueLength,
            "The previous value");

        ValidateOptionalText(
            newValue,
            MaximumValueLength,
            "The new value");

        ValidateRequiredText(
            performedBy,
            MaximumActorLength,
            "The history actor");

        return new IncidentHistoryEntry(
            Guid.NewGuid(),
            incidentId,
            eventType,
            description.Trim(),
            previousValue?.Trim(),
            newValue?.Trim(),
            performedBy.Trim(),
            occurredAt);
    }

    private static void ValidateRequiredText(
        string value,
        int maximumLength,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(
                $"{fieldName} is required.");
        }

        if (value.Trim().Length > maximumLength)
        {
            throw new DomainException(
                $"{fieldName} cannot exceed {maximumLength} characters.");
        }
    }

    private static void ValidateOptionalText(
        string? value,
        int maximumLength,
        string fieldName)
    {
        if (value is not null &&
            value.Trim().Length > maximumLength)
        {
            throw new DomainException(
                $"{fieldName} cannot exceed {maximumLength} characters.");
        }
    }
}
