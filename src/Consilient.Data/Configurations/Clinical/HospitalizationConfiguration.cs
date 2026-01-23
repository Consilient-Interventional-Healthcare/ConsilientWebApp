using Consilient.Data.Entities;
using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical
{
    internal class HospitalizationConfiguration : BaseEntityTypeConfigurationWithId<Hospitalization, int>
    {
        public override void Configure(EntityTypeBuilder<Hospitalization> entity)
        {
            base.Configure(entity);
            entity.ToTable("Hospitalizations", ConsilientDbContext.Schemas.Clinical);

            entity.HasAlternateKey(e => e.CaseId).HasName("AK_Hospitalizations_CaseId");

            entity.HasAlternateKey(e => new { e.CaseId, e.PatientId }).HasName("AK_Hospitalizations_CaseId_PatientId");

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.CaseId)
                .IsRequired();

            entity.Property(e => e.FacilityId)
                .IsRequired();

            entity.Property(e => e.AdmissionDate)
                   .IsRequired()
                   .HasColumnType("datetime2");

            entity.Property(e => e.DischargeDate)
                   .IsRequired(false)
                   .HasColumnType("datetime2");

            entity.HasOne(e => e.Patient)
                   .WithMany()
                   .HasForeignKey(e => e.PatientId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Hospitalizations_Patients_PatientId");

            entity.HasOne(e => e.Facility)
                   .WithMany()
                   .HasForeignKey(e => e.FacilityId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Hospitalizations_Facilities_FacilityId");

            entity.HasIndex(e => e.PatientId)
                   .HasDatabaseName("IX_Hospitalizations_PatientId");

            entity.HasIndex(e => e.FacilityId)
                   .HasDatabaseName("IX_Hospitalizations_FacilityId");

        }
    }
}
