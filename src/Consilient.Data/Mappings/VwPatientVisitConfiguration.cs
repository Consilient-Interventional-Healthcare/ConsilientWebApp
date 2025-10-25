using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class VwPatientVisitConfiguration : IEntityTypeConfiguration<VwPatientVisit>
    {
        public void Configure(EntityTypeBuilder<VwPatientVisit> entity)
        {
            entity.HasNoKey()
                .ToView("vw_PatientVisits", "Clinical");

            entity.Property(e => e.FacilityName).HasMaxLength(100);
            entity.Property(e => e.Insurance).HasMaxLength(113);
            entity.Property(e => e.NursePractitioner).HasMaxLength(105);
            entity.Property(e => e.PatientName).HasMaxLength(101);
            entity.Property(e => e.PatientVisitId).HasColumnName("PatientVisitID");
            entity.Property(e => e.Physician).HasMaxLength(105);
            entity.Property(e => e.Scribe).HasMaxLength(105);
            entity.Property(e => e.ServiceType).HasMaxLength(133);
        }
    }
}