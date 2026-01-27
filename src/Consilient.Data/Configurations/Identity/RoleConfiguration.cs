using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity;

internal class RoleConfiguration : BaseEntityTypeConfiguration<Role>
{
    internal const string TableName = "Roles";

    public override void Configure(EntityTypeBuilder<Role> entity)
    {
        base.Configure(entity);

        entity.ToTable(TableName, UsersDbContext.Schemas.Identity);
    }
}