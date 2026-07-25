using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public sealed class ChangeIncidentStatusCommandHandler
    : IRequestHandler<
        ChangeIncidentStatusCommand,
        ChangeIncidentStatusResult?>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly TimeProvider _timeProvider;

    public ChangeIncidentStatusCommandHandler(
        ISecurityIncidentRepository repository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
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
                incident.Close(_timeProvider.GetUtcNow());
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

        return new ChangeIncidentStatusResult(
            incident.Id,
            incident.Status,
            incident.ClosedAt);
    }
}
