using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SentinelCase.Domain.Entities;

namespace SentinelCase.Infrastructure.Persistence.Configurations;

internal sealed class IncidentNoteConfiguration
    : IEntityTypeConfiguration<IncidentNote>
{
    public void Configure(
        EntityTypeBuilder<IncidentNote> builder)
    {
        builder.ToTable("IncidentNotes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.Id)
            .ValueGeneratedNever();

        builder.Property(note => note.IncidentId)
            .IsRequired();

        builder.Property(note => note.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(note => note.CreatedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(note => note.CreatedAt)
            .IsRequired();

        builder.HasIndex(note => note.IncidentId);

        builder.HasIndex(note => note.CreatedAt);

        builder.HasOne<SecurityIncident>()
            .WithMany()
            .HasForeignKey(note => note.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
