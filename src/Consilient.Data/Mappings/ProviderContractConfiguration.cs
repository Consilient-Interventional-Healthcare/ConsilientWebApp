using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class ProviderContractConfiguration : IEntityTypeConfiguration<ProviderContract>
    {
        public void Configure(EntityTypeBuilder<ProviderContract> entity)
        {
            entity.ToTable("ProviderContracts", "Compensation");

            entity.Property(e => e.ProviderContractId).HasColumnName("ProviderContractID");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

            entity.HasOne(d => d.Contract).WithMany()
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Contract");

            entity.HasOne(d => d.Employee).WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProviderContracts_Employee");
        }
    }
}