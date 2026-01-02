using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class ServiceTypeConfiguration : BaseEntityTypeConfiguration<ServiceType>
    {
        public override void Configure(EntityTypeBuilder<ServiceType> entity)
        {
            base.Configure(entity);
            entity.ToTable("ServiceTypes", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Cptcode).IsRequired().HasColumnName("CPTCode");
            entity.Property(e => e.Description).IsRequired().HasMaxLength(100);
        }
    }
}