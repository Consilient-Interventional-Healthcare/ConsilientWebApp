using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class ProviderConfiguration : BaseEntityTypeConfigurationWithId<Provider, int>
    {
        public override void Configure(EntityTypeBuilder<Provider> entity)
        {
            base.Configure(entity);
            entity.ToTable("Providers", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TitleExtension).HasMaxLength(10);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeId);

            // Optional one-to-one with Employee (FK only, no navigation)
            entity.HasOne<Employee>()
                  .WithOne()
                  .HasForeignKey<Provider>(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_Providers_Employees_EmployeeId");

            entity.HasIndex(e => e.EmployeeId)
                  .IsUnique()
                  .HasFilter("[EmployeeId] IS NOT NULL")
                  .HasDatabaseName("IX_Providers_EmployeeId");
        }
    }
}
