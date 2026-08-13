using Microsoft.EntityFrameworkCore;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Repositories;

internal sealed class IncidentNoteRepository
    : IIncidentNoteRepository
{
    private readonly ApplicationDbContext _dbContext;

    public IncidentNoteRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        IncidentNote note,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IncidentNotes.AddAsync(
            note,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<IncidentNote>>
        GetByIncidentIdAsync(
            Guid incidentId,
            CancellationToken cancellationToken = default)
    {
        var notes = await _dbContext.IncidentNotes
            .AsNoTracking()
            .Where(note => note.IncidentId == incidentId)
            .ToListAsync(cancellationToken);

        return notes
            .OrderBy(note => note.CreatedAt)
            .ThenBy(note => note.Id)
            .ToArray();
    }
}
