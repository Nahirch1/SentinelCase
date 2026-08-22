using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Application.Common.Models;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;

namespace SentinelCase.UnitTests.TestDoubles;

internal sealed class FakeSecurityIncidentRepository
    : ISecurityIncidentRepository
{
    private readonly List<SecurityIncident> _incidents = [];

    public IReadOnlyCollection<SecurityIncident> Incidents => _incidents;

    public Task AddAsync(
        SecurityIncident incident,
        CancellationToken cancellationToken = default)
    {
        _incidents.Add(incident);
        return Task.CompletedTask;
    }

    public Task<SecurityIncident?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var incident = _incidents.SingleOrDefault(item => item.Id == id);
        return Task.FromResult(incident);
    }

    public Task UpdateAsync(
        SecurityIncident incident,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<PagedResult<SecurityIncident>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        string? searchTerm = null,
        string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<SecurityIncident> query = _incidents;

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
                    incident.Title.Contains(
                        normalizedSearchTerm,
                        StringComparison.OrdinalIgnoreCase) ||
                    incident.Description.Contains(
                        normalizedSearchTerm,
                        StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(assignedTo))
        {
            var normalizedAssignedTo = assignedTo.Trim();

            query = query.Where(
                incident => string.Equals(
                    incident.AssignedTo,
                    normalizedAssignedTo,
                    StringComparison.OrdinalIgnoreCase));
        }

        var filteredIncidents = query.ToArray();

        var incidents = filteredIncidents
            .OrderByDescending(incident => incident.CreatedAt)
            .ThenByDescending(incident => incident.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var result = new PagedResult<SecurityIncident>(
            incidents,
            pageNumber,
            pageSize,
            filteredIncidents.Length);

        return Task.FromResult(result);
    }

    public Task<IncidentSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var summary = new IncidentSummary(
            _incidents.Count,
            _incidents.Count(x => x.Status == IncidentStatus.Open),
            _incidents.Count(x => x.Severity == IncidentSeverity.Critical),
            _incidents.Count(x => x.Status == IncidentStatus.UnderInvestigation),
            _incidents.Count(x => x.Status == IncidentStatus.Contained),
            _incidents.Count(x => x.Status == IncidentStatus.Resolved),
            _incidents.Count(x => x.Status == IncidentStatus.Closed),
            _incidents.Count(x => x.Severity == IncidentSeverity.Low),
            _incidents.Count(x => x.Severity == IncidentSeverity.Medium),
            _incidents.Count(x => x.Severity == IncidentSeverity.High),
            _incidents.Count(x => x.Severity == IncidentSeverity.Critical));

        return Task.FromResult(summary);
    }

    public Task<bool> ExistsWithTitleAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        var exists = _incidents.Any(
            incident => string.Equals(
                incident.Title,
                title,
                StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<bool> ExistsWithTitleAsync(
        string title,
        Guid excludedIncidentId,
        CancellationToken cancellationToken = default)
    {
        var exists = _incidents.Any(
            incident =>
                incident.Id != excludedIncidentId &&
                string.Equals(
                    incident.Title,
                    title,
                    StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }
}
