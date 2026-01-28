using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class FacilityPayConfiguration : IEntityTypeConfiguration<FacilityPay>
    {
        public void Configure(EntityTypeBuilder<FacilityPay> entity)
        {
            entity.HasKey(e => e.FacilityPayId).HasName("PK_FacilityPays");

            entity.ToTable("FacilityPay", "Compensation");

            entity.Property(e => e.FacilityPayId).HasColumnName("FacilityPayID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.RevenueAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Type).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Facility).WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_Facility");

            // FK to ServiceTypes lookup table (uses enum property as FK)
            entity.HasOne(d => d.ServiceTypeNavigation).WithMany()
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_ServiceType");
        }
    }
}
