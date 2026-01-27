using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity;

internal class RoleClaimConfiguration : BaseEntityTypeConfiguration<RoleClaim>
{
    public override void Configure(EntityTypeBuilder<RoleClaim> entity)
    {
        base.Configure(entity);

        entity.ToTable("RoleClaims", UsersDbContext.Schemas.Identity);
    }
}