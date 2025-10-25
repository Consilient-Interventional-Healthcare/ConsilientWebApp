using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class FacilityConfiguration : IEntityTypeConfiguration<Facility>
    {
        public void Configure(EntityTypeBuilder<Facility> entity)
        {
            entity.ToTable("Facilities", "Clinical");

            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.FacilityAbbreviation).HasMaxLength(10);
            entity.Property(e => e.FacilityName).HasMaxLength(100);
        }
    }
}