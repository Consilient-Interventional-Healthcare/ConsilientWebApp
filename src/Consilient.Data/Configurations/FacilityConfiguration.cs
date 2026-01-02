using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class FacilityConfiguration : BaseEntityTypeConfiguration<Facility>
    {
        public override void Configure(EntityTypeBuilder<Facility> entity)
        {
            base.Configure(entity);
            entity.ToTable("Facilities", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Abbreviation).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        }
    }
}