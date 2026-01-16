using Consilient.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal class VisitAttendantConfiguration : BaseEntityTypeConfigurationWithId<VisitAttendant, int>
    {
        public override void Configure(EntityTypeBuilder<VisitAttendant> entity)
        {
            base.Configure(entity);
            entity.ToTable("VisitAttendants", ConsilientDbContext.Schemas.Clinical);
            entity.HasAlternateKey(e => new { e.VisitId, e.ProviderId })
                  .HasName("AK_VisitAttendants_VisitId_ProviderId");
            entity.Property(m => m.ProviderId).IsRequired();
            entity.Property(m => m.VisitId).IsRequired();

            entity.HasOne(e => e.Visit)
                  .WithMany(h => h.VisitAttendants)
                  .HasForeignKey(e => e.VisitId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_VisitAttendants_Visits_VisitId");

            entity.HasOne(e => e.Provider)
                  .WithMany()
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_VisitAttendants_Providers_ProviderId");

            entity.HasIndex(e => e.VisitId)
                  .HasDatabaseName("IX_VisitAttendants_VisitId");

            entity.HasIndex(e => e.ProviderId)
                  .HasDatabaseName("IX_VisitAttendants_ProviderId");
        }
    }
}
