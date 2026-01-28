using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class VisitEventTypeEntityConfiguration : BaseEntityTypeConfiguration<VisitEventTypeEntity>
{
    public override void Configure(EntityTypeBuilder<VisitEventTypeEntity> entity)
    {
        base.Configure(entity);

        entity.ToTable("VisitEventTypes", ConsilientDbContext.Schemas.Clinical);

        // Non-identity primary key - matches enum values
        entity.HasKey(e => e.Id).HasName("PK_VisitEventType");
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
            .HasDatabaseName("IX_VisitEventTypes_Code");
    }
}
