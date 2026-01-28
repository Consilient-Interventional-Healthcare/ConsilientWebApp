using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class ProviderTypeEntityConfiguration : BaseEntityTypeConfiguration<ProviderTypeEntity>
{
    public override void Configure(EntityTypeBuilder<ProviderTypeEntity> entity)
    {
        base.Configure(entity);

        entity.ToTable("ProviderTypes", ConsilientDbContext.Schemas.Clinical);

        // Non-identity primary key - matches enum values
        entity.HasKey(e => e.Id).HasName("PK_ProviderTypes");
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
            .HasDatabaseName("IX_ProviderTypes_Code");
    }
}
