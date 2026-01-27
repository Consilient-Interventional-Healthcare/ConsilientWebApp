using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class VisitEventTypeConfiguration : BaseEntityTypeConfigurationWithId<VisitEventType, int>
{
    public override void Configure(EntityTypeBuilder<VisitEventType> entity)
    {
        base.Configure(entity);
        entity.ToTable("VisitEventTypes", ConsilientDbContext.Schemas.Clinical);

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_VisitEventTypes_Code");
    }
}
