using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Mappings
{
    internal class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> entity)
        {
            entity.ToTable("Patients", "Clinical");

            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.PatientFirstName).HasMaxLength(50);
            entity.Property(e => e.PatientFullName)
                .HasMaxLength(101)
                .HasComputedColumnSql("((isnull([PatientFirstName],'')+' ')+isnull([PatientLastName],''))", false);
            entity.Property(e => e.PatientLastName).HasMaxLength(50);
            entity.Property(e => e.PatientMrn).HasColumnName("PatientMRN");
        }
    }
}