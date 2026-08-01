using Microsoft.EntityFrameworkCore;

using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Application.Common.Models;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;

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
            .AsNoTracking()
            .SingleOrDefaultAsync(
                incident => incident.Id == id,
                cancellationToken);
    }

    public async Task UpdateAsync(
        SecurityIncident incident,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SecurityIncidents.Update(incident);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<SecurityIncident>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SecurityIncidents
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(
                incident => incident.Status == status.Value);
        }

        if (severity.HasValue)
        {
            query = query.Where(
                incident => incident.Severity == severity.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim();

            query = query.Where(
                incident =>
                    incident.Title.Contains(normalizedSearchTerm) ||
                    incident.Description.Contains(normalizedSearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var incidents = await query
            .OrderByDescending(incident => incident.CreatedAt)
            .ThenByDescending(incident => incident.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SecurityIncident>(
            incidents,
            pageNumber,
            pageSize,
            totalCount);
    }

    public Task<bool> ExistsWithTitleAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SecurityIncidents
            .AsNoTracking()
            .AnyAsync(
                incident => incident.Title == title,
                cancellationToken);
    }

    public Task<bool> ExistsWithTitleAsync(
        string title,
        Guid excludedIncidentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SecurityIncidents
            .AsNoTracking()
            .AnyAsync(
                incident =>
                    incident.Id != excludedIncidentId &&
                    incident.Title == title,
                cancellationToken);
    }
}
