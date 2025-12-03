using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity
{
    internal class UserTokenConfiguration : BaseEntityTypeConfiguration<UserToken>
    {
        public override void Configure(EntityTypeBuilder<UserToken> entity)
        {
            base.Configure(entity);

            entity.ToTable("UserTokens", UsersDbContext.Schemas.Identity);

            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider)
                .HasMaxLength(128);
            entity.Property(e => e.Name)
                .HasMaxLength(128);
        }
    }
}