using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Configurations;

internal sealed class IncidentHistoryEntryConfiguration
    : IEntityTypeConfiguration<IncidentHistoryEntry>
{
    public void Configure(
        EntityTypeBuilder<IncidentHistoryEntry> builder)
    {
        builder.ToTable("IncidentHistoryEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.IncidentId)
            .IsRequired();

        builder.Property(entry => entry.EventType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(entry => entry.PreviousValue)
            .HasMaxLength(500);

        builder.Property(entry => entry.NewValue)
            .HasMaxLength(500);

        builder.Property(entry => entry.PerformedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.OccurredAt)
            .IsRequired();

        builder.HasIndex(entry => entry.IncidentId);

        builder.HasIndex(entry => entry.OccurredAt);

        builder.HasOne<SecurityIncident>()
            .WithMany()
            .HasForeignKey(entry => entry.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
