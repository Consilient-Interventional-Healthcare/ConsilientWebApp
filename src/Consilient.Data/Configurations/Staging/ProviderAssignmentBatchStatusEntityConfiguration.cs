using Consilient.Data.Entities.Staging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Staging;

internal class ProviderAssignmentBatchStatusEntityConfiguration : BaseEntityTypeConfiguration<ProviderAssignmentBatchStatusEntity>
{
    public override void Configure(EntityTypeBuilder<ProviderAssignmentBatchStatusEntity> entity)
    {
        base.Configure(entity);

        entity.ToTable("ProviderAssignmentBatchStatuses", "staging");

        // Non-identity primary key - matches enum values
        entity.HasKey(e => e.Id).HasName("PK_ProviderAssignmentBatchStatuses");
        entity.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.DisplayOrder)
            .IsRequired();

        entity.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_ProviderAssignmentBatchStatuses_Code");
    }
}
