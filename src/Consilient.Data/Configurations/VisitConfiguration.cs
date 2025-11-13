using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VisitConfiguration : BaseEntityTypeConfigurationWithId<Visit, int>
    {
        public override void Configure(EntityTypeBuilder<Visit> entity)
        {
            base.Configure(entity);
            entity.ToTable("Visits", ConsilientDbContext.Schemas.Clinical);

            entity.Property(e => e.HospitalizationId).IsRequired();
            entity.Property(e => e.DateServiced).IsRequired().HasColumnType("date");
            entity.Property(e => e.ServiceTypeId).IsRequired();
            entity.Property(e => e.IsScribeServiceOnly).HasDefaultValue(false);
            entity.Property(e => e.Room).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Bed).IsRequired().HasMaxLength(5);

            entity.HasOne(d => d.Hospitalization).WithMany()
                .HasForeignKey(d => d.HospitalizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visits_Hospitalizations_HospitalizationId");

            entity.HasOne(d => d.ServiceType).WithMany()
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visits_ServiceTypes_ServiceTypeId");

            entity.HasIndex(e => e.DateServiced, "IX_Visits_DateServiced");
            entity.HasIndex(e => e.ServiceTypeId).HasDatabaseName("IX_Visits_ServiceTypeId");


        }
    }
}