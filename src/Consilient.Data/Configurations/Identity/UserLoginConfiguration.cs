using Consilient.Data.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity
{
    internal class UserLoginConfiguration : BaseEntityTypeConfiguration<UserLogin>
    {
        public override void Configure(EntityTypeBuilder<UserLogin> entity)
        {
            base.Configure(entity);

            entity.ToTable("UserLogins", UsersDbContext.Schemas.Identity);

            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            // Optional: column sizing / constraints to match provider expectations
            entity.Property(e => e.LoginProvider).HasMaxLength(128).IsRequired();
            entity.Property(e => e.ProviderKey).HasMaxLength(128).IsRequired();
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(256);

            //// Enforce uniqueness of (LoginProvider, ProviderKey)
            //entity.HasIndex(e => new { e.LoginProvider, e.ProviderKey })
            //      .IsUnique()
            //      .HasDatabaseName("UQ_AspNetUserLogins_LoginProvider_ProviderKey");
        }
    }
}