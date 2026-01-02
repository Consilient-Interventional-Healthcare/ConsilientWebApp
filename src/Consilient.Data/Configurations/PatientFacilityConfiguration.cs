using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class PatientFacilityConfiguration : BaseEntityTypeConfigurationWithId<PatientFacility, int>
    {
        public override void Configure(EntityTypeBuilder<PatientFacility> entity)
        {
            base.Configure(entity);
            entity.ToTable("PatientFacilities", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.Mrn)
                .IsRequired()
                .HasColumnName("MRN");

            entity.HasOne(d => d.Patient)
                .WithMany(p => p.PatientFacilities)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientFacilities_Patients");

            entity.HasOne(d => d.Facility)
                .WithMany(f => f.PatientFacilities)
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientFacilities_Facilities");

            // Ensure MRN is unique per facility, not globally unique
            entity.HasIndex(e => new { e.FacilityId, e.Mrn })
                .IsUnique()
                .HasDatabaseName("IX_PatientFacilities_FacilityId_MRN");
        }
    }
}
