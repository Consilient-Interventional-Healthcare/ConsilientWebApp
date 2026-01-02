using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class PayrollDatumConfiguration : IEntityTypeConfiguration<PayrollDatum>
    {
        public void Configure(EntityTypeBuilder<PayrollDatum> entity)
        {
            entity.HasKey(e => e.PayrollDataId).HasName("PK_PayrollDatum");

            entity.ToTable("PayrollData", "Compensation");

            entity.Property(e => e.PayrollDataId).HasColumnName("PayrollDataID");
            entity.Property(e => e.PayrollPeriodId).HasColumnName("PayrollPeriodID");
            entity.Property(e => e.ProviderPayId).HasColumnName("ProviderPayID");

            entity.HasOne(d => d.PayrollPeriod).WithMany()
                .HasForeignKey(d => d.PayrollPeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayrollData_PayrollPeriod");

            entity.HasOne(d => d.ProviderPay).WithMany()
                .HasForeignKey(d => d.ProviderPayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PayrollData_ProviderPay");
        }
    }
}