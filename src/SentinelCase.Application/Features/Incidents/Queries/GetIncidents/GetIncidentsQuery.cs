using MediatR;

using SentinelCase.Application.Common.Models;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

public sealed record GetIncidentsQuery(
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<GetIncidentsItem>>;
