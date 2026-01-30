using Consilient.Data.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Billing;

internal class ServiceTypeBillingCodeConfiguration : BaseEntityTypeConfigurationWithId<ServiceTypeBillingCode, int>
{
    public override void Configure(EntityTypeBuilder<ServiceTypeBillingCode> entity)
    {
        base.Configure(entity);

        entity.ToTable("ServiceTypeBillingCodes", ConsilientDbContext.Schemas.Billing);

        // Unique constraint: one pairing per (ServiceType, BillingCode)
        entity.HasAlternateKey(e => new { e.ServiceTypeId, e.BillingCodeId })
            .HasName("AK_ServiceTypeBillingCodes_ServiceTypeId_BillingCodeId");

        // FK to ServiceTypes lookup table
        entity.HasOne(e => e.ServiceType)
            .WithMany()
            .HasForeignKey(e => e.ServiceTypeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ServiceTypeBillingCodes_ServiceTypes");

        entity.HasOne(e => e.BillingCode)
            .WithMany()
            .HasForeignKey(e => e.BillingCodeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ServiceTypeBillingCodes_BillingCodes");

        // Indexes
        entity.HasIndex(e => e.ServiceTypeId).HasDatabaseName("IX_ServiceTypeBillingCodes_ServiceTypeId");
        entity.HasIndex(e => e.BillingCodeId).HasDatabaseName("IX_ServiceTypeBillingCodes_BillingCodeId");

        // IsDefault
        entity.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
    }
}
