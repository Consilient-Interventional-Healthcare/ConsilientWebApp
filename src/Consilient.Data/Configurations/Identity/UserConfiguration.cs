using Consilient.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Consilient.Data.Configurations.Identity;

internal class UserConfiguration : BaseEntityTypeConfiguration<User>
{
    internal const string TableName = "Users";

    public override void Configure(EntityTypeBuilder<User> entity)
    {
        base.Configure(entity);

        entity.ToTable(TableName, UsersDbContext.Schemas.Identity);

        entity.HasIndex(u => u.NormalizedEmail)
              .IsUnique()
              .HasDatabaseName("UQ_Identity_Users_NormalizedEmail")
              .HasFilter("[NormalizedEmail] IS NOT NULL");

        entity.HasIndex(u => u.NormalizedUserName)
              .IsUnique()
              .HasDatabaseName("UQ_Identity_Users_NormalizedUserName")
              .HasFilter("[NormalizedUserName] IS NOT NULL");
    }
}