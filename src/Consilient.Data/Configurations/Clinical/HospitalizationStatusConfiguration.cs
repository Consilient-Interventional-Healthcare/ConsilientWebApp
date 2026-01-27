using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.Configurations.Clinical;

internal class HospitalizationStatusConfiguration : BaseEntityTypeConfigurationWithId<HospitalizationStatus, int>
{
    public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<HospitalizationStatus> entity)
    {
        base.Configure(entity);
        entity.ToTable("HospitalizationStatuses", ConsilientDbContext.Schemas.Clinical);
        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        entity.Property(e => e.Color)
            .IsRequired()
            .HasMaxLength(20);

        // Specify the related entity type explicitly
        entity.HasMany<Hospitalization>() // Replace 'Hospitalization' with the correct related entity type
            .WithOne(hs => hs.HospitalizationStatus)
            .HasForeignKey(hs => hs.HospitalizationStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
