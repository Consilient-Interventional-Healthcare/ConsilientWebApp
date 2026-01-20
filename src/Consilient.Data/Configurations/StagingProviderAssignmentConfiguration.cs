using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class StagingProviderAssignmentConfiguration : BaseEntityTypeConfiguration<ProviderAssignment>
    {
        public override void Configure(EntityTypeBuilder<ProviderAssignment> entity)
        {
            base.Configure(entity);
            entity.ToTable("ProviderAssignments", "staging");

            entity.Property(e => e.Age).IsRequired();
            entity.Property(e => e.AttendingMD).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HospitalNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Admit).IsRequired().HasColumnType("smalldatetime");
            entity.Property(e => e.Dob).HasColumnType("date");
            entity.Property(e => e.FacilityId);
            entity.Property(e => e.Mrn).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Insurance).HasMaxLength(255);
            entity.Property(e => e.NursePractitioner).HasMaxLength(255);
            entity.Property(e => e.IsCleared).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.H_P).HasMaxLength(255);
            entity.Property(e => e.PsychEval).HasMaxLength(255);
            entity.Property(e => e.ResolvedPhysicianId);
            entity.Property(e => e.ResolvedHospitalizationId);
            entity.Property(e => e.ResolvedPatientId);
            entity.Property(e => e.ResolvedNursePractitionerId);
            entity.Property(e => e.BatchId).IsRequired();
            entity.Property(e => e.Imported).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.ValidationErrorsJson)
                .HasColumnName("ValidationErrors")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ExclusionReason).HasMaxLength(500);
            entity.Property(e => e.ShouldImport).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.ResolvedVisitId);
            entity.Property(e => e.NormalizedPatientLastName).HasMaxLength(100);
            entity.Property(e => e.NormalizedPatientFirstName).HasMaxLength(100);
            entity.Property(e => e.NormalizedPhysicianLastName).HasMaxLength(100);
            entity.Property(e => e.NormalizedNursePractitionerLastName).HasMaxLength(100);
            entity.Property(e => e.PatientFacilityWasCreated).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.HospitalizationWasCreated).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.NursePractitionerWasCreated).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.PhysicianWasCreated).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.PatientWasCreated).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.Room).HasMaxLength(20);
            entity.Property(e => e.Bed).HasMaxLength(5);
            entity.Ignore(e => e.ValidationErrors);

        }
    }
}
