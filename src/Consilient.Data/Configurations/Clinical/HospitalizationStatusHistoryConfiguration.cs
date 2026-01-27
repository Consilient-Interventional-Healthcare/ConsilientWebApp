using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class HospitalizationStatusHistoryConfiguration : BaseEntityTypeConfigurationWithId<HospitalizationStatusHistory, int>
{
    public override void Configure(EntityTypeBuilder<HospitalizationStatusHistory> entity)
    {
        base.Configure(entity);
        entity.ToTable("HospitalizationStatusHistories", ConsilientDbContext.Schemas.Clinical);

        entity.Property(e => e.HospitalizationId)
            .IsRequired();

        entity.Property(e => e.NewStatusId)
            .IsRequired();

        entity.Property(e => e.ChangedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(e => e.ChangedByUserId);

        entity.HasOne<Hospitalization>()
            .WithMany()
            .HasForeignKey(e => e.HospitalizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HospitalizationStatusHistories_Hospitalizations_HospitalizationId");

        entity.HasOne<HospitalizationStatus>()
            .WithMany()
            .HasForeignKey(e => e.NewStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId");

        entity.HasIndex(e => e.HospitalizationId)
            .HasDatabaseName("IX_HospitalizationStatusHistories_HospitalizationId");

        entity.HasIndex(e => e.NewStatusId)
            .HasDatabaseName("IX_HospitalizationStatusHistories_NewStatusId");

        entity.HasIndex(e => e.ChangedAt)
            .HasDatabaseName("IX_HospitalizationStatusHistories_ChangedAt");

        entity.HasIndex(e => e.ChangedByUserId)
            .HasDatabaseName("IX_HospitalizationStatusHistories_ChangedByUserId");
    }
}