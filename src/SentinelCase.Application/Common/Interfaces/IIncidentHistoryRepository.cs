using SentinelCase.Domain.Entities;

namespace SentinelCase.Application.Common.Interfaces;

public interface IIncidentHistoryRepository
{
    Task AddAsync(
        IncidentHistoryEntry entry,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IncidentHistoryEntry>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default);
}
