using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

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
}