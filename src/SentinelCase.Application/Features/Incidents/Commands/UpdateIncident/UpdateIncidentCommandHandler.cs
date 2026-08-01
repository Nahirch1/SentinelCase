using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;

public sealed class UpdateIncidentCommandHandler
    : IRequestHandler<UpdateIncidentCommand, UpdateIncidentResult?>
{
    private readonly ISecurityIncidentRepository _repository;

    public UpdateIncidentCommandHandler(
        ISecurityIncidentRepository repository)
    {
        _repository = repository;
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

        var titleChanged = !string.Equals(
            incident.Title,
            normalizedTitle,
            StringComparison.OrdinalIgnoreCase);

        if (titleChanged)
        {
            var titleAlreadyExists =
                await _repository.ExistsWithTitleAsync(
                    normalizedTitle,
                    cancellationToken);

            if (titleAlreadyExists)
            {
                throw new DomainException(
                    "An incident with the same title already exists.");
            }
        }

        incident.UpdateDetails(
            request.Title,
            request.Description);

        incident.ChangeSeverity(request.Severity);

        await _repository.UpdateAsync(
            incident,
            cancellationToken);

        return new UpdateIncidentResult(
            incident.Id,
            incident.Title,
            incident.Description,
            incident.Severity,
            incident.Status);
    }
}
