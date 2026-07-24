using MediatR;

using SentinelCase.Application.Common.Interfaces;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;

public sealed class GetIncidentByIdQueryHandler
    : IRequestHandler<GetIncidentByIdQuery, GetIncidentByIdResult?>
{
    private readonly ISecurityIncidentRepository _repository;

    public GetIncidentByIdQueryHandler(
        ISecurityIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetIncidentByIdResult?> Handle(
        GetIncidentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        return new GetIncidentByIdResult(
            incident.Id,
            incident.Title,
            incident.Description,
            incident.Severity,
            incident.Status,
            incident.DetectedAt,
            incident.CreatedAt);
    }
}
