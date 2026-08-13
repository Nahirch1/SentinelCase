using SentinelCase.Domain.Entities;

namespace SentinelCase.Application.Common.Interfaces;

public interface IIncidentNoteRepository
{
    Task AddAsync(
        IncidentNote note,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IncidentNote>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default);
}
