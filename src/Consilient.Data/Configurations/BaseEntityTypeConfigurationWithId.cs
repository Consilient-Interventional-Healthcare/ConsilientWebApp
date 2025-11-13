using Consilient.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations
{
    internal abstract class BaseEntityTypeConfigurationWithId<TEntity, TID> : BaseEntityTypeConfiguration<TEntity>
        where TEntity : class, IEntity<TID>, IAuditableEntity
        where TID : struct, IEquatable<TID>
    {
        public override void Configure(EntityTypeBuilder<TEntity> entity)
        {
            base.Configure(entity);
            entity.HasKey(e => e.Id).HasName($"PK_{typeof(TEntity).Name}");
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        }
    }
}
