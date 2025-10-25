using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class PatientVisitConfiguration : IEntityTypeConfiguration<PatientVisit>
    {
        public void Configure(EntityTypeBuilder<PatientVisit> entity)
        {
            entity.ToTable("PatientVisits", "Clinical");

            entity.Property(e => e.PatientVisitId).HasColumnName("PatientVisitID");
            entity.Property(e => e.CosigningPhysicianEmployeeId).HasColumnName("CosigningPhysicianEmployeeID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.IsSupervising).HasComputedColumnSql("(case when [NursePractitionerEmployeeID] IS NULL then (0) else (1) end)", false);
            entity.Property(e => e.NursePractitionerEmployeeId).HasColumnName("NursePractitionerEmployeeID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PhysicianEmployeeId).HasColumnName("PhysicianEmployeeID");
            entity.Property(e => e.ScribeEmployeeId).HasColumnName("ScribeEmployeeID");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany(p => p.PatientVisitCosigningPhysicianEmployees)
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Facility");

            entity.HasOne(d => d.Insurance).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Insurances");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany(p => p.PatientVisitNursePractitionerEmployees)
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany(p => p.PatientVisitPhysicianEmployees)
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany(p => p.PatientVisitScribeEmployees)
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.PatientVisits)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_ServiceType");
        }
    }
}