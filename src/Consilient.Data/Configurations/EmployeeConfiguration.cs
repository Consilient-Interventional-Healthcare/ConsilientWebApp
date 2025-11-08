using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> entity)
        {
            entity.ToTable("Employees", "Compensation");
            entity.Property(e => e.Id).HasColumnName("EmployeeID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.FullName)
                .HasMaxLength(105)
                .HasComputedColumnSql("(case when [TitleExtension] IS NULL then (isnull([FirstName],'')+' ')+isnull([LastName],'') else (((isnull([FirstName],'')+' ')+isnull([LastName],''))+', ')+isnull([TitleExtension],'') end)", false);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.TitleExtension).HasMaxLength(2);
        }
    }
}