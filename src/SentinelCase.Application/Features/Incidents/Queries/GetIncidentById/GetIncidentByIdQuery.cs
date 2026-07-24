using MediatR;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;

public sealed record GetIncidentByIdQuery(
    Guid Id)
    : IRequest<GetIncidentByIdResult?>;
