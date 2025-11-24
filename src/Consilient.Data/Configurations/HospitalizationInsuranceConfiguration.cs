using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class HospitalizationInsuranceConfiguration : BaseEntityTypeConfigurationWithId<HospitalizationInsurance, int>
    {
        public override void Configure(EntityTypeBuilder<HospitalizationInsurance> entity)
        {
            base.Configure(entity);
            entity.ToTable("HospitalizationInsurances", ConsilientDbContext.Schemas.Clinical);
            entity.Property(e => e.StartDate).IsRequired();

            entity.HasOne(d => d.Hospitalization)
                .WithMany()
                .HasForeignKey(d => d.HospitalizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HospitalizationInsurances_Hospitalizations");

            entity.HasOne(d => d.Insurance)
                .WithMany()
                .HasForeignKey(d => d.InsuranceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HospitalizationInsurances_Insurances");
        }
    }
}
