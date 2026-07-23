using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Application.Features.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentCommandHandler
    : IRequestHandler<CreateIncidentCommand, CreateIncidentResult>
{
    private readonly ISecurityIncidentRepository _repository;
    private readonly TimeProvider _timeProvider;

    public CreateIncidentCommandHandler(
        ISecurityIncidentRepository repository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<CreateIncidentResult> Handle(
        CreateIncidentCommand request,
        CancellationToken cancellationToken)
    {
        var titleAlreadyExists = await _repository.ExistsWithTitleAsync(
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

        await _repository.AddAsync(incident, cancellationToken);

        return new CreateIncidentResult(
            incident.Id,
            incident.Title,
            incident.Severity,
            incident.Status,
            incident.DetectedAt,
            incident.CreatedAt);
    }
}