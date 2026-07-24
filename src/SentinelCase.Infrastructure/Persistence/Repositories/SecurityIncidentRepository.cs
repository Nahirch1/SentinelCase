using Microsoft.EntityFrameworkCore;
using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Repositories;

internal sealed class SecurityIncidentRepository
    : ISecurityIncidentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SecurityIncidentRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SecurityIncident incident,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SecurityIncidents.AddAsync(
            incident,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<SecurityIncident?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SecurityIncidents
            .SingleOrDefaultAsync(
                incident => incident.Id == id,
                cancellationToken);
    }

    public Task<bool> ExistsWithTitleAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SecurityIncidents
            .AnyAsync(
                incident => incident.Title == title,
                cancellationToken);
    }
}
