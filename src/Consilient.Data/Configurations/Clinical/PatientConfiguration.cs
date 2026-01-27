using Consilient.Data.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Clinical;

internal class PatientConfiguration : BaseEntityTypeConfiguration<Patient>
{
    public override void Configure(EntityTypeBuilder<Patient> entity)
    {
        base.Configure(entity);
        entity.ToTable("Patients", ConsilientDbContext.Schemas.Clinical);

        entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
    }
}