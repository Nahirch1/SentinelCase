using MediatR;

using SentinelCase.Application.Common.Models;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentSummary;

public sealed record GetIncidentSummaryQuery
    : IRequest<IncidentSummary>;
