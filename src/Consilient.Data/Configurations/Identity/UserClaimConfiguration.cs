using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity
{

    internal class UserClaimConfiguration : BaseEntityTypeConfiguration<UserClaim>
    {
        public override void Configure(EntityTypeBuilder<UserClaim> entity)
        {
            base.Configure(entity);

            entity.ToTable("UserClaims", UsersDbContext.Schemas.Identity);
        }
    }
}