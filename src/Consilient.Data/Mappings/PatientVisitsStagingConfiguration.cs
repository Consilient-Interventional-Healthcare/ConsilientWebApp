using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class PatientVisitsStagingConfiguration : IEntityTypeConfiguration<PatientVisitsStaging>
    {
        public void Configure(EntityTypeBuilder<PatientVisitsStaging> entity)
        {
            entity.HasKey(e => e.PatientVisitStagingId);

            entity.ToTable("PatientVisits_Staging", "Clinical");

            entity.Property(e => e.PatientVisitStagingId).HasColumnName("PatientVisit_StagingID");
            entity.Property(e => e.CosigningPhysicianEmployeeId).HasColumnName("CosigningPhysicianEmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.NursePractitionerEmployeeId).HasColumnName("NursePractitionerEmployeeID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PhysicianApprovedBy).HasMaxLength(100);
            entity.Property(e => e.PhysicianApprovedDateTime).HasColumnType("datetime");
            entity.Property(e => e.PhysicianEmployeeId).HasColumnName("PhysicianEmployeeID");
            entity.Property(e => e.ScribeEmployeeId).HasColumnName("ScribeEmployeeID");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany(p => p.PatientVisitsStagingCosigningPhysicianEmployees)
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Facility");

            entity.HasOne(d => d.Insurance).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Staging_Insurance");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany(p => p.PatientVisitsStagingNursePractitionerEmployees)
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany(p => p.PatientVisitsStagingPhysicianEmployees)
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany(p => p.PatientVisitsStagingScribeEmployees)
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.PatientVisitsStagings)
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_PatientVisits_Staging_ServiceType");
        }
    }
}