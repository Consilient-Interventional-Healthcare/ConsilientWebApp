using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
    {
        public void Configure(EntityTypeBuilder<ServiceType> entity)
        {
            entity.ToTable("ServiceTypes", "Clinical");

            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");
            entity.Property(e => e.CodeAndDescription)
                .HasMaxLength(133)
                .HasComputedColumnSql("((isnull(CONVERT([nvarchar],[CPTCode]),'')+' - ')+isnull([Description],''))", false);
            entity.Property(e => e.Cptcode).HasColumnName("CPTCode");
            entity.Property(e => e.Description).HasMaxLength(100);
        }
    }
}