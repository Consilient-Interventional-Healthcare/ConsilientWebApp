using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations;

internal class InsuranceConfiguration : BaseEntityTypeConfiguration<Insurance>
{
    public override void Configure(EntityTypeBuilder<Insurance> entity)
    {
        base.Configure(entity);
        entity.ToTable("Insurances", ConsilientDbContext.Schemas.Clinical);
        entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
        entity.Property(e => e.Description).IsRequired().HasMaxLength(100);
        entity.Property(e => e.IsContracted).HasDefaultValue(false);
        entity.Property(e => e.PhysicianIncluded).HasDefaultValue(false);
    }
}