using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Configurations;

internal sealed class SecurityIncidentConfiguration
    : IEntityTypeConfiguration<SecurityIncident>
{
    public void Configure(
        EntityTypeBuilder<SecurityIncident> builder)
    {
        builder.ToTable("SecurityIncidents");

        builder.HasKey(incident => incident.Id);

        builder.Property(incident => incident.Id)
            .ValueGeneratedNever();

        builder.Property(incident => incident.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(incident => incident.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(incident => incident.Severity)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(incident => incident.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(incident => incident.DetectedAt)
            .IsRequired();

        builder.Property(incident => incident.CreatedAt)
            .IsRequired();

        builder.Property(incident => incident.ClosedAt);

        builder.HasIndex(incident => incident.Title)
            .IsUnique();

        builder.HasIndex(incident => incident.Status);

        builder.HasIndex(incident => incident.Severity);

        builder.HasIndex(incident => incident.CreatedAt);
    }
}
