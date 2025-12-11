using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
{
    /// <inheritdoc />
    public partial class Initial
    {
        /// <inheritdoc />
        private static void SeedData(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                schema: "Clinical",
                table: "HospitalizationStatuses",
                columns: ["Id", "Code", "Name", "BillingCode", "Color", "DisplayOrder", "CreatedAtUtc", "UpdatedAtUtc"],
                values: new object[,]
                {
                    { 1, "DTS", "Acute", "99233", "#64ffda", 1, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 2, "DTO", "Acute", "99233", "#64ffda", 2, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 3, "GD", "Acute", "99233", "#64ffda", 3, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 4, "SND", "Status Next Day", "99232", "#ffd180", 4, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 5, "DC", "Discharge Summary", "99239", "#bbdefb", 5, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 6, "PP", "Pending Placement", "", "#e0e0e0", 6, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 7, "TCON-PP", "", "", "", 7, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) },
                    { 8, "PE", "Psychiatric Evaluation", "90792", "#fff176", 8, new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc), new DateTime(2025, 11, 25, 2, 53, 10, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        private static void UnseedData(MigrationBuilder migrationBuilder)
        {
        }
    }
}
