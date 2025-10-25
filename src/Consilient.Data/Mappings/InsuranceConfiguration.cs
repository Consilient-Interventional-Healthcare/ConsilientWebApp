using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
    {
        public void Configure(EntityTypeBuilder<Insurance> entity)
        {
            entity.ToTable("Insurances", "Clinical");

            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.CodeAndDescription)
                .HasMaxLength(113)
                .HasComputedColumnSql("((isnull([InsuranceCode],'')+' - ')+isnull([InsuranceDescription],''))", false);
            entity.Property(e => e.InsuranceCode).HasMaxLength(10);
            entity.Property(e => e.InsuranceDescription).HasMaxLength(100);
            entity.Property(e => e.IsContracted).HasDefaultValue(false);
            entity.Property(e => e.PhysicianIncluded).HasDefaultValue(false);
        }
    }
}