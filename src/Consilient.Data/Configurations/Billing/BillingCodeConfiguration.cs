using Consilient.Data.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Billing;

internal class BillingCodeConfiguration : BaseEntityTypeConfiguration<BillingCode>
{
    public override void Configure(EntityTypeBuilder<BillingCode> entity)
    {
        base.Configure(entity);

        entity.ToTable("BillingCodes", ConsilientDbContext.Schemas.Billing);

        entity.HasKey(e => e.Id).HasName("PK_BillingCodes");
        entity.Property(e => e.Id).IsRequired().ValueGeneratedNever();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasColumnType("varchar(20)");
        entity.HasAlternateKey(e => e.Code).HasName("AK_BillingCodes_Code");

        entity.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(200);
    }
}
