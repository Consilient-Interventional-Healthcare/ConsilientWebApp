using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
    {
        public void Configure(EntityTypeBuilder<ServiceType> entity)
        {
            entity.ToTable("ServiceTypes", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Id).HasColumnName("ServiceTypeID");
            entity.Property(e => e.CodeAndDescription)
                .HasMaxLength(133)
                .HasComputedColumnSql("((isnull(CONVERT([nvarchar],[CPTCode]),'')+' - ')+isnull([Description],''))", false);
            entity.Property(e => e.Cptcode).HasColumnName("CPTCode");
            entity.Property(e => e.Description).HasMaxLength(100);
        }
    }
}