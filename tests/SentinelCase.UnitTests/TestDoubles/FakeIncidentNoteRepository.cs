using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Domain.Entities;

namespace SentinelCase.UnitTests.TestDoubles;

internal sealed class FakeIncidentNoteRepository
    : IIncidentNoteRepository
{
    private readonly List<IncidentNote> _notes = [];

    public IReadOnlyCollection<IncidentNote> Notes => _notes;

    public Task AddAsync(
        IncidentNote note,
        CancellationToken cancellationToken = default)
    {
        _notes.Add(note);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IncidentNote>>
        GetByIncidentIdAsync(
            Guid incidentId,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<IncidentNote> notes =
            _notes
                .Where(note => note.IncidentId == incidentId)
                .OrderBy(note => note.CreatedAt)
                .ThenBy(note => note.Id)
                .ToArray();

        return Task.FromResult(notes);
    }
}
