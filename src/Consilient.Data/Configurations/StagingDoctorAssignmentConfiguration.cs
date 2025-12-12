using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Consilient.Data.Configurations
{
    internal class StagingDoctorAssignmentConfiguration : BaseEntityTypeConfiguration<DoctorAssignment>
    {
        public override void Configure(EntityTypeBuilder<DoctorAssignment> entity)
        {
            base.Configure(entity);
            entity.ToTable("DoctorAssignments", "staging");

            entity.Property(e => e.Age).IsRequired();
            entity.Property(e => e.AttendingMD).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HospitalNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Admit).IsRequired();
            entity.Property(e => e.FacilityId);
            entity.Property(e => e.Mrn).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Insurance).HasMaxLength(255);
            entity.Property(e => e.NursePractitioner).HasMaxLength(255);
            entity.Property(e => e.IsCleared).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.H_P).HasMaxLength(255);
            entity.Property(e => e.PsychEval).HasMaxLength(255);
            entity.Property(e => e.ResolvedProviderId);
            entity.Property(e => e.ResolvedHospitalizationId);
            entity.Property(e => e.ResolvedPatientId);
            entity.Property(e => e.ResolvedNursePracticionerId);
            entity.Property(e => e.BatchId).IsRequired();
            entity.Property(e => e.Imported).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.ValidationErrors)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            entity.Property(e => e.ExclusionReason).HasMaxLength(500);
            entity.Property(e => e.ShouldImport).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.NeedsNewPatient).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.NeedsNewHospitalization).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.ResolvedVisitId);
        }
    }
}
