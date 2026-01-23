using Consilient.Data.Entities;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.Entities.Compensation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Compensation
{
    internal class ProviderContractConfiguration : BaseEntityTypeConfigurationWithId<ProviderContract, int>
    {
        public override void Configure(EntityTypeBuilder<ProviderContract> entity)
        {
            base.Configure(entity);
            entity.ToTable("ProviderContracts", ConsilientDbContext.Schemas.Compensation);

            //entity.Property(e => e.ContractId)
            //    .HasColumnName("ContractID")
            //    .IsRequired();

            entity.Property(e => e.EmployeeId)
                .HasColumnName("EmployeeID")
                .IsRequired();

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate)
                .IsRequired();

            entity.Property(e => e.FacilityId)
                .IsRequired();
            //entity.HasOne(d => d.Contract)
            //    .WithMany()
            //    .HasForeignKey(d => d.ContractId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_ProviderContracts_Contract");

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Employee");

            entity.HasOne<Facility>()
                .WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Facility");
        }
    }
}
