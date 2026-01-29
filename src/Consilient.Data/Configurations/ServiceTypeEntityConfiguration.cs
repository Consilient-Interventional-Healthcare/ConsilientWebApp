using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations;

internal class ServiceTypeEntityConfiguration : BaseEntityTypeConfiguration<ServiceTypeEntity>
{
    public override void Configure(EntityTypeBuilder<ServiceTypeEntity> entity)
    {
        base.Configure(entity);

        entity.ToTable("ServiceTypes", ConsilientDbContext.Schemas.Clinical);

        // Non-identity primary key - matches enum values
        entity.HasKey(e => e.Id).HasName("PK_ServiceType");
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
            .HasDatabaseName("IX_ServiceTypes_Code");
    }
}
