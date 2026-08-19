using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SentinelCase.Domain.Entities;
using SentinelCase.Infrastructure.Identity;

namespace SentinelCase.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>
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

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
