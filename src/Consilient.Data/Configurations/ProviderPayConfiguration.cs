using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class ProviderPayConfiguration : IEntityTypeConfiguration<ProviderPay>
    {
        public void Configure(EntityTypeBuilder<ProviderPay> entity)
        {
            entity.ToTable("ProviderPay", "Compensation");

            entity.Property(e => e.ProviderPayId).HasColumnName("ProviderPayID");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.PayAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PayUnit).HasMaxLength(100);
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Employee).WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderPay_Employee");

            entity.HasOne(d => d.Facility).WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderPay_Facility");

            entity.HasOne(d => d.ServiceType).WithMany()
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_ProviderPay_ServiceType");
        }
    }
}