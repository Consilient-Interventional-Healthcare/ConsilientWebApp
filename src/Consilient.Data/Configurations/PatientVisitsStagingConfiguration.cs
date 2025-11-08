using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class PatientVisitsStagingConfiguration : IEntityTypeConfiguration<PatientVisitStaging>
    {
        public void Configure(EntityTypeBuilder<PatientVisitStaging> entity)
        {
            entity.ToTable("PatientVisits_Staging", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Id).HasColumnName("PatientVisit_StagingID");
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

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany()
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Facility");

            entity.HasOne(d => d.Insurance).WithMany()
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Staging_Insurance");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany()
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany()
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany()
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Staging_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany()
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Staging_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany()
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_PatientVisits_Staging_ServiceType");
        }
    }
}