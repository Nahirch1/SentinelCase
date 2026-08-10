using Microsoft.EntityFrameworkCore;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Repositories;

internal sealed class IncidentHistoryRepository
    : IIncidentHistoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public IncidentHistoryRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        IncidentHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IncidentHistoryEntries.AddAsync(
            entry,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<IncidentHistoryEntry>>
        GetByIncidentIdAsync(
            Guid incidentId,
            CancellationToken cancellationToken = default)
    {
        var entries = await _dbContext.IncidentHistoryEntries
            .AsNoTracking()
            .Where(entry => entry.IncidentId == incidentId)
            .ToListAsync(cancellationToken);

        return entries
            .OrderBy(entry => entry.OccurredAt)
            .ThenBy(entry => entry.EventType)
            .ThenBy(entry => entry.Id)
            .ToArray();
    }
}
