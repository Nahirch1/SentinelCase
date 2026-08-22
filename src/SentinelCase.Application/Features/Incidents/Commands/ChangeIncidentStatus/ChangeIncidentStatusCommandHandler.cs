using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public sealed class ChangeIncidentStatusCommandHandler
    : IRequestHandler<
        ChangeIncidentStatusCommand,
        ChangeIncidentStatusResult?>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly IIncidentHistoryRepository _historyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ISentinelCaseMetrics _metrics;

    public ChangeIncidentStatusCommandHandler(
        ISecurityIncidentRepository repository,
        IIncidentHistoryRepository historyRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        ISentinelCaseMetrics metrics)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _metrics = metrics;
    }

    public async Task<ChangeIncidentStatusResult?> Handle(
        ChangeIncidentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        if (incident.Status == request.Status)
        {
            return new ChangeIncidentStatusResult(
                incident.Id,
                incident.Status,
                incident.ClosedAt);
        }

        var previousStatus = incident.Status;
        var occurredAt = _timeProvider.GetUtcNow();

        switch (request.Status)
        {
            case IncidentStatus.UnderInvestigation:
                incident.StartInvestigation();
                break;

            case IncidentStatus.Contained:
                incident.Contain();
                break;

            case IncidentStatus.Resolved:
                incident.Resolve();
                break;

            case IncidentStatus.Closed:
                incident.Close(occurredAt);
                break;

            case IncidentStatus.Open:
                throw new DomainException(
                    "An incident cannot return to the open status.");

            default:
                throw new DomainException(
                    "The requested incident status is invalid.");
        }

        await _repository.UpdateAsync(
            incident,
            cancellationToken);

        var eventType =
            incident.Status == IncidentStatus.Closed
                ? IncidentHistoryEventType.Closed
                : IncidentHistoryEventType.StatusChanged;

        var historyEntry = IncidentHistoryEntry.Create(
            incident.Id,
            eventType,
            incident.Status == IncidentStatus.Closed
                ? "The incident was closed."
                : "The incident status was changed.",
            previousValue: previousStatus.ToString(),
            newValue: incident.Status.ToString(),
            _currentUser.Identifier,
            occurredAt);

        await _historyRepository.AddAsync(
            historyEntry,
            cancellationToken);

        _metrics.RecordIncidentStatusChanged(
            previousStatus,
            incident.Status);

        return new ChangeIncidentStatusResult(
            incident.Id,
            incident.Status,
            incident.ClosedAt);
    }
}
