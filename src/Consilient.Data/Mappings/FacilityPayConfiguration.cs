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
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Facility).WithMany(p => p.FacilityPays)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_Facility");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.FacilityPays)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacilityPay_ServiceType");
        }
    }
}