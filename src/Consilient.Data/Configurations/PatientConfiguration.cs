using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> entity)
        {
            entity.ToTable("Patients", ConsilientDbContext.Schemas.Clinical);

            entity.HasAlternateKey(e => e.PatientMrn)
                .HasName("AK_Patients_PatientMrn");

            entity.Property(e => e.Id).HasColumnName("PatientID");
            entity.Property(e => e.PatientFirstName).HasMaxLength(50);
            entity.Property(e => e.PatientFullName)
                .HasMaxLength(101)
                .HasComputedColumnSql("((isnull([PatientFirstName],'')+' ')+isnull([PatientLastName],''))", false);
            entity.Property(e => e.PatientLastName).HasMaxLength(50);
            entity.Property(e => e.PatientMrn).HasColumnName("PatientMRN");
        }
    }
}