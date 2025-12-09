using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
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
                .IsRequired();

            entity.Property(e => e.EventRecordedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.EnteredByEmployeeId)
                .IsRequired();

            entity.HasOne(e => e.Visit)
                .WithMany(v => v.VisitEvents)
                .HasForeignKey(e => e.VisitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_VisitEvents_Visits_VisitId");

            entity.HasOne(e => e.EventType)
                .WithMany(et => et.VisitEvents)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_VisitEvents_VisitEventTypes_EventTypeId");

            entity.HasOne(e => e.EnteredByEmployee)
                .WithMany(emp => emp.EnteredVisitEvents)
                .HasForeignKey(e => e.EnteredByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_VisitEvents_Employees_EnteredByEmployeeId");

            entity.HasIndex(e => e.VisitId)
                .HasDatabaseName("IX_VisitEvents_VisitId");

            entity.HasIndex(e => e.EventTypeId)
                .HasDatabaseName("IX_VisitEvents_EventTypeId");

            entity.HasIndex(e => e.EventOccurredAt)
                .HasDatabaseName("IX_VisitEvents_EventOccurredAt");

            entity.HasIndex(e => e.EnteredByEmployeeId)
                .HasDatabaseName("IX_VisitEvents_EnteredByEmployeeId");
        }
    }
}
