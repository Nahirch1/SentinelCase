using MediatR;

using SentinelCase.Application.Common.Models;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

public sealed record GetIncidentsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    IncidentStatus? Status = null,
    IncidentSeverity? Severity = null,
    string? SearchTerm = null,
    string? AssignedTo = null)
    : IRequest<PagedResult<GetIncidentsItem>>;
