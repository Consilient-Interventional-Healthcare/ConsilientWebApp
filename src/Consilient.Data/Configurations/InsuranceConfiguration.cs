using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
    {
        public void Configure(EntityTypeBuilder<Insurance> entity)
        {
            entity.ToTable("Insurances", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Id).HasColumnName("InsuranceID");
            entity.Property(e => e.InsuranceCode).HasMaxLength(10);
            entity.Property(e => e.InsuranceDescription).HasMaxLength(100);
            entity.Property(e => e.IsContracted).HasDefaultValue(false);
            entity.Property(e => e.PhysicianIncluded).HasDefaultValue(false);
        }
    }
}