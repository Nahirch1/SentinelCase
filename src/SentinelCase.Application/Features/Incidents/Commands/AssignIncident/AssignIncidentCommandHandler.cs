using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.AssignIncident;

public sealed class AssignIncidentCommandHandler
    : IRequestHandler<AssignIncidentCommand, AssignIncidentResult?>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly IIncidentHistoryRepository _historyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public AssignIncidentCommandHandler(
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

    public async Task<AssignIncidentResult?> Handle(
        AssignIncidentCommand request,
        CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        var previousAssignee = incident.AssignedTo;
        var assignedAt = _timeProvider.GetUtcNow();

        incident.AssignTo(
            request.AnalystIdentifier,
            assignedAt);

        await _repository.UpdateAsync(
            incident,
            cancellationToken);

        var historyEntry = IncidentHistoryEntry.Create(
            incident.Id,
            IncidentHistoryEventType.Assigned,
            "The incident was assigned to an analyst.",
            previousValue: previousAssignee,
            newValue: incident.AssignedTo,
            _currentUser.Identifier,
            assignedAt);

        await _historyRepository.AddAsync(
            historyEntry,
            cancellationToken);

        return new AssignIncidentResult(
            incident.Id,
            incident.AssignedTo!,
            incident.AssignedAt!.Value);
    }
}
