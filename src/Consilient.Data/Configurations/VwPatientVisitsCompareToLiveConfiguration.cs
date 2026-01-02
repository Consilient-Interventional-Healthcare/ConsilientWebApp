using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VwPatientVisitsCompareToLiveConfiguration : IEntityTypeConfiguration<VwPatientVisitsCompareToLive>
    {
        public void Configure(EntityTypeBuilder<VwPatientVisitsCompareToLive> entity)
        {
            entity.HasNoKey()
                .ToView("vw_PatientVisits_CompareToLive", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.AttendingPhysicianJoinId)
                .HasMaxLength(50)
                .HasColumnName("AttendingPhysicianJoinID");
            entity.Property(e => e.CaseId).HasColumnName("CaseID");
            entity.Property(e => e.CosignPhysicianJoinId)
                .HasMaxLength(50)
                .HasColumnName("CosignPhysicianJoinID");
            entity.Property(e => e.Cptcd).HasColumnName("CPTCD");
            entity.Property(e => e.ImportFileNm).HasColumnName("ImportFileNM");
            entity.Property(e => e.InsuranceNm)
                .HasMaxLength(100)
                .HasColumnName("InsuranceNM");
            entity.Property(e => e.ModifiedDts).HasColumnName("ModifiedDTS");
            entity.Property(e => e.Mrn).HasColumnName("MRN");
            entity.Property(e => e.NursePractitionerJoinId)
                .HasMaxLength(50)
                .HasColumnName("NursePractitionerJoinID");
            entity.Property(e => e.PatientBirthDts).HasColumnName("PatientBirthDTS");
            entity.Property(e => e.PatientNm)
                .HasMaxLength(101)
                .HasColumnName("PatientNM");
            entity.Property(e => e.ScribeNm)
                .HasMaxLength(50)
                .HasColumnName("ScribeNM");
            entity.Property(e => e.ServiceDts).HasColumnName("ServiceDTS");
        }
    }
}