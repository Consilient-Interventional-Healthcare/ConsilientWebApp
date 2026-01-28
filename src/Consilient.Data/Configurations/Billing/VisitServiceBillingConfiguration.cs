using Consilient.Data.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Billing;

internal class VisitServiceBillingConfiguration : BaseEntityTypeConfigurationWithId<VisitServiceBilling, int>
{
    public override void Configure(EntityTypeBuilder<VisitServiceBilling> entity)
    {
        base.Configure(entity);

        entity.ToTable("VisitServiceBillings", ConsilientDbContext.Schemas.Billing);

        entity.Property(e => e.Type).HasColumnName("ServiceTypeId");

        entity.HasAlternateKey(e => new { e.VisitId, e.Type, e.BillingCodeId })
            .HasName("AK_VisitServiceBillings_VisitId_ServiceTypeId_BillingCodeId");

        entity.HasOne(e => e.Visit)
            .WithMany()
            .HasForeignKey(e => e.VisitId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VisitServiceBillings_Visits_VisitId");

        // FK to ServiceTypes lookup table (uses enum property as FK)
        entity.HasOne(e => e.ServiceTypeNavigation)
            .WithMany()
            .HasForeignKey(e => e.Type)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VisitServiceBillings_ServiceTypes_ServiceTypeId");

        entity.HasOne(e => e.BillingCode)
            .WithMany()
            .HasForeignKey(e => e.BillingCodeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VisitServiceBillings_BillingCodes_BillingCodeId");

        entity.HasIndex(e => e.VisitId).HasDatabaseName("IX_VisitServiceBillings_VisitId");
        entity.HasIndex(e => e.Type).HasDatabaseName("IX_VisitServiceBillings_ServiceTypeId");
        entity.HasIndex(e => e.BillingCodeId).HasDatabaseName("IX_VisitServiceBillings_BillingCodeId");
    }
}
