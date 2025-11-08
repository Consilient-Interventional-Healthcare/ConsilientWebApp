using Consilient.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal abstract class BaseEntityTypeConfiguration<TEntity, TID> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IEntity<TID>, IAuditableEntity
        where TID : struct, IEquatable<TID>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> entity)
        {
            entity.HasKey(e => e.Id).HasName($"PK_{typeof(TEntity).Name}");
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(a => a.CreatedAtUtc).IsRequired();
            entity.Property(a => a.UpdatedAtUtc).IsRequired();

            entity.Property(e => e.RowVersion)
                   .IsRowVersion()
                   .IsRequired()
                   .HasColumnName("RowVersion")
                   .HasColumnType("rowversion");
        }
    }
}
