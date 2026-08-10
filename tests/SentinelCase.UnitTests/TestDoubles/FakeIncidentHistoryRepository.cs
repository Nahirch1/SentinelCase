using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

namespace SentinelCase.UnitTests.TestDoubles;

internal sealed class FakeIncidentHistoryRepository
    : IIncidentHistoryRepository
{
    private readonly List<IncidentHistoryEntry> _entries = [];

    public IReadOnlyCollection<IncidentHistoryEntry> Entries =>
        _entries;

    public Task AddAsync(
        IncidentHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        _entries.Add(entry);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IncidentHistoryEntry>>
        GetByIncidentIdAsync(
            Guid incidentId,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<IncidentHistoryEntry> entries =
            _entries
                .Where(entry => entry.IncidentId == incidentId)
                .OrderBy(entry => entry.OccurredAt)
                .ThenBy(entry => entry.Id)
                .ToArray();

        return Task.FromResult(entries);
    }
}
