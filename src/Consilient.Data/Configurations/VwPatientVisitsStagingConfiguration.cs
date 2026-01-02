using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VwPatientVisitsStagingConfiguration : IEntityTypeConfiguration<VwPatientVisitsStaging>
    {
        public void Configure(EntityTypeBuilder<VwPatientVisitsStaging> entity)
        {
            entity.HasNoKey()
                .ToView("vw_PatientVisits_Staging", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.FacilityName).HasMaxLength(100);
            entity.Property(e => e.Insurance).HasMaxLength(113);
            entity.Property(e => e.NursePractitioner).HasMaxLength(105);
            entity.Property(e => e.PatientName).HasMaxLength(101);
            entity.Property(e => e.PatientVisitStagingId).HasColumnName("PatientVisit_StagingID");
            entity.Property(e => e.Physician).HasMaxLength(105);
            entity.Property(e => e.Scribe).HasMaxLength(105);
            entity.Property(e => e.ServiceType).HasMaxLength(133);
        }
    }
}