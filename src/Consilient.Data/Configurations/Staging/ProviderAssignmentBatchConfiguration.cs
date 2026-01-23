using Consilient.Data.Entities;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.Entities.Staging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Staging
{
    internal class ProviderAssignmentBatchConfiguration : BaseEntityTypeConfiguration<ProviderAssignmentBatch>
    {
        public override void Configure(EntityTypeBuilder<ProviderAssignmentBatch> entity)
        {
            base.Configure(entity);

            entity.ToTable("ProviderAssignmentBatches", "staging");

            // Primary Key - GUID provided externally, NOT auto-generated
            entity.HasKey(e => e.Id).HasName("PK_ProviderAssignmentBatches");
            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedNever();

            entity.Property(e => e.Date)
                .IsRequired()
                .HasColumnType("date");

            entity.Property(e => e.FacilityId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(ProviderAssignmentBatchStatus.Pending);

            // Indexes
            entity.HasIndex(e => new { e.FacilityId, e.Date })
                .HasDatabaseName("IX_ProviderAssignmentBatches_FacilityId_Date");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_ProviderAssignmentBatches_Status");
        }
    }
}
