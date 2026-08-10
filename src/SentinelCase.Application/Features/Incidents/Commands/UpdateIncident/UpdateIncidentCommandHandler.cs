using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;

public sealed class UpdateIncidentCommandHandler
    : IRequestHandler<UpdateIncidentCommand, UpdateIncidentResult?>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly IIncidentHistoryRepository _historyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdateIncidentCommandHandler(
        ISecurityIncidentRepository repository,
        IIncidentHistoryRepository historyRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<UpdateIncidentResult?> Handle(
        UpdateIncidentCommand request,
        CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        var normalizedTitle = request.Title.Trim();
        var normalizedDescription = request.Description.Trim();

        var titleAlreadyExists =
            await _repository.ExistsWithTitleAsync(
                normalizedTitle,
                incident.Id,
                cancellationToken);

        if (titleAlreadyExists)
        {
            throw new DomainException(
                "An incident with the same title already exists.");
        }

        var previousTitle = incident.Title;
        var previousDescription = incident.Description;
        var previousSeverity = incident.Severity;

        var detailsChanged =
            !string.Equals(
                previousTitle,
                normalizedTitle,
                StringComparison.Ordinal) ||
            !string.Equals(
                previousDescription,
                normalizedDescription,
                StringComparison.Ordinal);

        var severityChanged =
            previousSeverity != request.Severity;

        incident.UpdateDetails(
            request.Title,
            request.Description);

        incident.ChangeSeverity(request.Severity);

        await _repository.UpdateAsync(
            incident,
            cancellationToken);

        var occurredAt = _timeProvider.GetUtcNow();

        if (detailsChanged)
        {
            var historyEntry = IncidentHistoryEntry.Create(
                incident.Id,
                IncidentHistoryEventType.DetailsUpdated,
                "The incident details were updated.",
                previousValue:
                    $"Title: {previousTitle}; Description: {previousDescription}",
                newValue:
                    $"Title: {incident.Title}; Description: {incident.Description}",
                _currentUser.Identifier,
                occurredAt);

            await _historyRepository.AddAsync(
                historyEntry,
                cancellationToken);
        }

        if (severityChanged)
        {
            var historyEntry = IncidentHistoryEntry.Create(
                incident.Id,
                IncidentHistoryEventType.SeverityChanged,
                "The incident severity was changed.",
                previousValue: previousSeverity.ToString(),
                newValue: incident.Severity.ToString(),
                _currentUser.Identifier,
                occurredAt);

            await _historyRepository.AddAsync(
                historyEntry,
                cancellationToken);
        }

        return new UpdateIncidentResult(
            incident.Id,
            incident.Title,
            incident.Description,
            incident.Severity,
            incident.Status,
            incident.DetectedAt,
            incident.CreatedAt,
            incident.ClosedAt);
    }
}
