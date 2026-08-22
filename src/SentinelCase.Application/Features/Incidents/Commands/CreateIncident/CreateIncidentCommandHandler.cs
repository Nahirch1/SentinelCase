using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentCommandHandler
    : IRequestHandler<CreateIncidentCommand, CreateIncidentResult>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly IIncidentHistoryRepository _historyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ISentinelCaseMetrics _metrics;

    public CreateIncidentCommandHandler(
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

    public async Task<CreateIncidentResult> Handle(
        CreateIncidentCommand request,
        CancellationToken cancellationToken)
    {
        var titleAlreadyExists =
            await _repository.ExistsWithTitleAsync(
                request.Title.Trim(),
                cancellationToken);

        if (titleAlreadyExists)
        {
            throw new DomainException(
                "An incident with the same title already exists.");
        }

        var createdAt = _timeProvider.GetUtcNow();

        var incident = SecurityIncident.Create(
            request.Title,
            request.Description,
            request.Severity,
            request.DetectedAt,
            createdAt);

        await _repository.AddAsync(
            incident,
            cancellationToken);

        var historyEntry = IncidentHistoryEntry.Create(
            incident.Id,
            IncidentHistoryEventType.Created,
            "The incident was created.",
            previousValue: null,
            newValue: incident.Status.ToString(),
            _currentUser.Identifier,
            createdAt);

        await _historyRepository.AddAsync(
            historyEntry,
            cancellationToken);

        _metrics.RecordIncidentCreated(
            incident.Severity);

        return new CreateIncidentResult(
            incident.Id,
            incident.Title,
            incident.Severity,
            incident.Status,
            incident.DetectedAt,
            incident.CreatedAt);
    }
}
