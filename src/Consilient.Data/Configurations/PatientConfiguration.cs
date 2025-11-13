using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class PatientConfiguration : BaseEntityTypeConfiguration<Patient>
    {
        public override void Configure(EntityTypeBuilder<Patient> entity)
        {
            base.Configure(entity);
            entity.ToTable("Patients", ConsilientDbContext.Schemas.Clinical);

            entity.HasAlternateKey(e => e.Mrn).HasName("AK_Patients_MRN");

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Mrn).IsRequired().HasColumnName("MRN");
        }
    }
}