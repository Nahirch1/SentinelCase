using MediatR;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;

public sealed record GetIncidentHistoryQuery(
    Guid IncidentId)
    : IRequest<IReadOnlyCollection<GetIncidentHistoryItem>?>;
