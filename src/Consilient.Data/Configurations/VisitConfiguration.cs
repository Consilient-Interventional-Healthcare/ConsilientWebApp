using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VisitConfiguration : BaseEntityTypeConfiguration<Visit, int>
    {
        public override void Configure(EntityTypeBuilder<Visit> entity)
        {
            base.Configure(entity);
            entity.ToTable("PatientVisits", ConsilientDbContext.Schemas.Clinical);

            //entity.HasAlternateKey(e => new { e.HospitalizationId, e.DateServiced, e.ServiceTypeId, e.PhysicianEmployeeId, e.NursePractitionerEmployeeId })
            //    .HasName("AK_PatientVisits_HospitalizationID_DateServiced_ServiceTypeID_PhysicianEmployeeID_NursePractitionerEmployeeID");

            entity.Property(e => e.Id).HasColumnName("PatientVisitID");
            entity.Property(e => e.HospitalizationId).IsRequired().HasColumnName("HospitalizationID");
            entity.Property(e => e.DateServiced).HasColumnType("date");
            entity.Property(e => e.CosigningPhysicianEmployeeId).HasColumnName("CosigningPhysicianEmployeeID");
            entity.Property(e => e.InsuranceId).HasColumnName("InsuranceID");
            entity.Property(e => e.NursePractitionerEmployeeId).HasColumnName("NursePractitionerEmployeeID");
            entity.Property(e => e.PhysicianEmployeeId).HasColumnName("PhysicianEmployeeID");
            entity.Property(e => e.ScribeEmployeeId).HasColumnName("ScribeEmployeeID");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.Hospitalization).WithMany()
                .HasForeignKey(d => d.HospitalizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Hospitalizations");

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany()
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_PatientVisits_CosignPhysicianEmployee");

            entity.HasOne(d => d.Insurance).WithMany()
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_PatientVisits_Insurances");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany()
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_PatientVisits_NursePractitioner");

            entity.HasOne(d => d.PhysicianEmployee).WithMany()
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany()
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_PatientVisits_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany()
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientVisits_ServiceType");

        }
    }
}