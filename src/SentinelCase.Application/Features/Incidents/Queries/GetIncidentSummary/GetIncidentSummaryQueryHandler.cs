using MediatR;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Application.Common.Models;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentSummary;

public sealed class GetIncidentSummaryQueryHandler
    : IRequestHandler<GetIncidentSummaryQuery, IncidentSummary>
{
    private readonly ISecurityIncidentRepository _repository;

    public GetIncidentSummaryQueryHandler(
        ISecurityIncidentRepository repository)
    {
        _repository = repository;
    }

    public Task<IncidentSummary> Handle(
        GetIncidentSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetSummaryAsync(
            cancellationToken);
    }
}
