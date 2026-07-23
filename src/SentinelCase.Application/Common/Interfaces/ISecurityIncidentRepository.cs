using SentinelCase.Domain.Entities;

namespace SentinelCase.Application.Common.Interfaces;

public interface ISecurityIncidentRepository
{
    Task AddAsync(
        SecurityIncident incident,
        CancellationToken cancellationToken = default);

    Task<SecurityIncident?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsWithTitleAsync(
        string title,
        CancellationToken cancellationToken = default);
}