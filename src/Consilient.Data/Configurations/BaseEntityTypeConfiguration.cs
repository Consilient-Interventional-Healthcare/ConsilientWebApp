using Consilient.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal abstract class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IAuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> entity)
        {
            entity.Property(a => a.CreatedAtUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(a => a.UpdatedAtUtc).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.RowVersion)
                   .IsRowVersion()
                   .IsRequired()
                   .HasColumnName("RowVersion")
                   .HasColumnType("rowversion");
        }
    }
}
