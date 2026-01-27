using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity;

internal class UserRoleConfiguration : BaseEntityTypeConfiguration<UserRole>
{
    internal const string TableName = "UserRoles";

    public override void Configure(EntityTypeBuilder<UserRole> entity)
    {
        base.Configure(entity);

        entity.ToTable(TableName, UsersDbContext.Schemas.Identity);

        entity.HasKey(e => new { e.UserId, e.RoleId });
    }
}