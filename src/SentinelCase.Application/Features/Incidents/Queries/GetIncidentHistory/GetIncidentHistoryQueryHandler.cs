using MediatR;

using SentinelCase.Application.Common.Interfaces;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;

public sealed class GetIncidentHistoryQueryHandler
    : IRequestHandler<
        GetIncidentHistoryQuery,
        IReadOnlyCollection<GetIncidentHistoryItem>?>
{
    private readonly ISecurityIncidentRepository _incidentRepository;
    private readonly IIncidentHistoryRepository _historyRepository;

    public GetIncidentHistoryQueryHandler(
        ISecurityIncidentRepository incidentRepository,
        IIncidentHistoryRepository historyRepository)
    {
        _incidentRepository = incidentRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IReadOnlyCollection<GetIncidentHistoryItem>?> Handle(
        GetIncidentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var incident = await _incidentRepository.GetByIdAsync(
            request.IncidentId,
            cancellationToken);

        if (incident is null)
        {
            return null;
        }

        var entries =
            await _historyRepository.GetByIncidentIdAsync(
                request.IncidentId,
                cancellationToken);

        return entries
            .Select(entry => new GetIncidentHistoryItem(
                entry.Id,
                entry.EventType,
                entry.Description,
                entry.PreviousValue,
                entry.NewValue,
                entry.PerformedBy,
                entry.OccurredAt))
            .ToArray();
    }
}
