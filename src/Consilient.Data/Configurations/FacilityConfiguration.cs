using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class FacilityConfiguration : IEntityTypeConfiguration<Facility>
    {
        public void Configure(EntityTypeBuilder<Facility> entity)
        {
            entity.ToTable("Facilities", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Id).HasColumnName("FacilityID");
            entity.Property(e => e.FacilityAbbreviation).HasMaxLength(10);
            entity.Property(e => e.FacilityName).HasMaxLength(100);
        }
    }
}