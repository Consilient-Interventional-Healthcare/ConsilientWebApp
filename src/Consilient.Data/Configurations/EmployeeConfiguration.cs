using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class EmployeeConfiguration : BaseEntityTypeConfiguration<Employee>
    {
        public override void Configure(EntityTypeBuilder<Employee> entity)
        {
            base.Configure(entity);
            entity.ToTable("Employees", "Compensation");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TitleExtension).HasMaxLength(2);
            entity.Property(e => e.IsProvider).HasDefaultValue(false);
            entity.Property(e => e.IsAdministrator).HasDefaultValue(false);
            entity.Property(e => e.CanApproveVisits).HasDefaultValue(false);
        }
    }
}
