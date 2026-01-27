using Consilient.Data.Configurations.Identity;
using Consilient.Data.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Users;

/// <inheritdoc />
public partial class Initial
{
    /// <inheritdoc />
    private static void SeedData(MigrationBuilder migrationBuilder)
    {

        var defaultPassword = "Hernan";

        var seedDate = new DateTime(2025, 11, 24, 0, 0, 0, DateTimeKind.Utc);

        migrationBuilder.InsertData(
            schema: UsersDbContext.Schemas.Identity,
            table: RoleConfiguration.TableName,
            columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp", "CreatedAtUtc", "UpdatedAtUtc"],
            values: new object[,]
            {
                { 1, RoleNames.Administrator, RoleNames.Administrator.ToUpperInvariant(), Guid.NewGuid().ToString(), seedDate, seedDate },
                { 2, RoleNames.Nurse, RoleNames.Nurse.ToUpperInvariant(), Guid.NewGuid().ToString(), seedDate, seedDate },
                { 3, RoleNames.Provider, RoleNames.Provider.ToUpperInvariant(), Guid.NewGuid().ToString(), seedDate, seedDate }
            });



        var hasher = new PasswordHasher<User>();

        var adminUser = new User { UserName = "administrator@local", NormalizedUserName = "ADMINISTRATOR@LOCAL", Email = "administrator@local", NormalizedEmail = "ADMINISTRATOR@LOCAL" };
        var nurseUser = new User { UserName = "nurse@local", NormalizedUserName = "NURSE@LOCAL", Email = "nurse@local", NormalizedEmail = "NURSE@LOCAL" };
        var providerUser = new User { UserName = "provider@local", NormalizedUserName = "PROVIDER@LOCAL", Email = "provider@local", NormalizedEmail = "PROVIDER@LOCAL" };

        var adminHash = hasher.HashPassword(adminUser, defaultPassword);
        var nurseHash = hasher.HashPassword(nurseUser, defaultPassword);
        var providerHash = hasher.HashPassword(providerUser, defaultPassword);

        migrationBuilder.InsertData(
            schema: UsersDbContext.Schemas.Identity,
            table: UserConfiguration.TableName,
            columns:
            [
                "Id",
                "UserName",
                "NormalizedUserName",
                "Email",
                "NormalizedEmail",
                "EmailConfirmed",
                "PasswordHash",
                "SecurityStamp",
                "ConcurrencyStamp",
                "CreatedAtUtc",
                "UpdatedAtUtc"
            ],
            values: new object[,]
            {
                { 100, adminUser.UserName, adminUser.NormalizedUserName, adminUser.Email, adminUser.NormalizedEmail, true, adminHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), seedDate, seedDate },
                { 101, nurseUser.UserName, nurseUser.NormalizedUserName, nurseUser.Email, nurseUser.NormalizedEmail, true, nurseHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), seedDate, seedDate },
                { 102, providerUser.UserName, providerUser.NormalizedUserName, providerUser.Email, providerUser.NormalizedEmail, true, providerHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), seedDate, seedDate }
            });

        // Link seeded users to seeded roles (role ids assumed 1=Administrator, 2=Nurse, 3=Provider)
        migrationBuilder.InsertData(
            schema: UsersDbContext.Schemas.Identity,
            table: UserRoleConfiguration.TableName,
            columns: ["UserId", "RoleId"],
            values: new object[,]
            {
                { 100, 1 },
                { 101, 2 },
                { 102, 3 }
            });
    }
    /// <inheritdoc />
    private static void UnseedData(MigrationBuilder migrationBuilder)
    {
    }
}
