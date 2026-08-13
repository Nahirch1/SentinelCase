using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Application.Common.Models;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

public sealed class GetIncidentsQueryHandler
    : IRequestHandler<GetIncidentsQuery, PagedResult<GetIncidentsItem>>
{
    private readonly ISecurityIncidentRepository _repository;

    public GetIncidentsQueryHandler(
        ISecurityIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<GetIncidentsItem>> Handle(
        GetIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.Severity,
            request.SearchTerm,
            request.AssignedTo,
            cancellationToken);

        var items = result.Items
            .Select(incident => new GetIncidentsItem(
                incident.Id,
                incident.Title,
                incident.Severity,
                incident.Status,
                incident.DetectedAt,
                incident.CreatedAt,
                incident.AssignedTo,
                incident.AssignedAt))
            .ToArray();

        return new PagedResult<GetIncidentsItem>(
            items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount);
    }
}
