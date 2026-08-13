using Microsoft.EntityFrameworkCore;
using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SecurityIncident> SecurityIncidents =>
        Set<SecurityIncident>();

    public DbSet<IncidentHistoryEntry> IncidentHistoryEntries =>
        Set<IncidentHistoryEntry>();

    public DbSet<IncidentNote> IncidentNotes =>
        Set<IncidentNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
