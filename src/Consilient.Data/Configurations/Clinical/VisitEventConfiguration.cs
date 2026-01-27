using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class VisitEventConfiguration : BaseEntityTypeConfigurationWithId<VisitEvent, int>
{
    public override void Configure(EntityTypeBuilder<VisitEvent> entity)
    {
        base.Configure(entity);
        entity.ToTable("VisitEvents", ConsilientDbContext.Schemas.Clinical);

        entity.Property(e => e.VisitId)
            .IsRequired();

        entity.Property(e => e.EventTypeId)
            .IsRequired();

        entity.Property(e => e.EventOccurredAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(e => e.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entity.HasOne<Visit>()
            .WithMany(v => v.VisitEvents)
            .HasForeignKey(e => e.VisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VisitEvents_Visits_VisitId");

        entity.HasOne<VisitEventType>()
            .WithMany(et => et.VisitEvents)
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VisitEvents_VisitEventTypes_EventTypeId");

        entity.HasIndex(e => e.VisitId)
            .HasDatabaseName("IX_VisitEvents_VisitId");

        entity.HasIndex(e => e.EventTypeId)
            .HasDatabaseName("IX_VisitEvents_EventTypeId");

        entity.HasIndex(e => e.EventOccurredAt)
            .HasDatabaseName("IX_VisitEvents_EventOccurredAt");

        entity.HasIndex(e => e.EnteredByUserId)
            .HasDatabaseName("IX_VisitEvents_EnteredByUserId");
    }
}
