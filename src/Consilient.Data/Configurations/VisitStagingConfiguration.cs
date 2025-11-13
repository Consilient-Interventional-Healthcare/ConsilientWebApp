using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VisitStagingConfiguration : BaseEntityTypeConfiguration<VisitStaging>
    {
        public override void Configure(EntityTypeBuilder<VisitStaging> entity)
        {
            base.Configure(entity);
            entity.ToTable("VisitsStaging", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.CosigningPhysicianEmployeeId);
            entity.Property(e => e.FacilityId);
            entity.Property(e => e.InsuranceId);
            entity.Property(e => e.NursePractitionerEmployeeId);
            entity.Property(e => e.NursePractitionerApproved).HasDefaultValue(false);

            entity.Property(e => e.PatientId);
            entity.Property(e => e.PhysicianApproved).HasDefaultValue(false);
            entity.Property(e => e.IsScribeServiceOnly).HasDefaultValue(false);
            entity.Property(e => e.AddedToMainTable).HasDefaultValue(false);
            entity.Property(e => e.PhysicianApprovedBy).HasMaxLength(100);
            entity.Property(e => e.PhysicianApprovedDateTime).HasColumnType("datetime");
            entity.Property(e => e.PhysicianEmployeeId);
            entity.Property(e => e.ScribeEmployeeId);
            entity.Property(e => e.ServiceTypeId);

            entity.HasOne(d => d.CosigningPhysicianEmployee).WithMany()
                .HasForeignKey(d => d.CosigningPhysicianEmployeeId)
                .HasConstraintName("FK_VisitsStaging_CosignPhysicianEmployee");

            entity.HasOne(d => d.Facility).WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitsStaging_Facility");

            entity.HasOne(d => d.Insurance).WithMany()
                .HasForeignKey(d => d.InsuranceId)
                .HasConstraintName("FK_VisitsStaging_Insurance");

            entity.HasOne(d => d.NursePractitionerEmployee).WithMany()
                .HasForeignKey(d => d.NursePractitionerEmployeeId)
                .HasConstraintName("FK_VisitsStaging_NursePractitioner");

            entity.HasOne(d => d.Patient).WithMany()
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitsStaging_Patient");

            entity.HasOne(d => d.PhysicianEmployee).WithMany()
                .HasForeignKey(d => d.PhysicianEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitsStaging_Physician");

            entity.HasOne(d => d.ScribeEmployee).WithMany()
                .HasForeignKey(d => d.ScribeEmployeeId)
                .HasConstraintName("FK_VisitsStaging_Scribe");

            entity.HasOne(d => d.ServiceType).WithMany()
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_VisitsStaging_ServiceType");
        }
    }
}