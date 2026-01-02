using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
    {
        public void Configure(EntityTypeBuilder<PayrollPeriod> entity)
        {
            entity.ToTable("PayrollPeriods", "Compensation");

            entity.Property(e => e.PayrollPeriodId).HasColumnName("PayrollPeriodID");
        }
    }
}