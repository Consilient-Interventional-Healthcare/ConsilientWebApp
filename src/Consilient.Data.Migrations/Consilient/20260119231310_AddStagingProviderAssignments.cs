using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
{
    /// <inheritdoc />
    public partial class AddStagingProviderAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "staging");

            migrationBuilder.CreateTable(
                name: "ProviderAssignments",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AttendingMD = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HospitalNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Admit = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Mrn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Insurance = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NursePractitioner = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsCleared = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    H_P = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PsychEval = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NormalizedPatientLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NormalizedPatientFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NormalizedPhysicianLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NormalizedNursePractitionerLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Room = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Bed = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ResolvedPhysicianId = table.Column<int>(type: "int", nullable: true),
                    ResolvedHospitalizationId = table.Column<int>(type: "int", nullable: true),
                    ResolvedPatientId = table.Column<int>(type: "int", nullable: true),
                    ResolvedNursePractitionerId = table.Column<int>(type: "int", nullable: true),
                    ResolvedVisitId = table.Column<int>(type: "int", nullable: true),
                    ResolvedFacilityId = table.Column<int>(type: "int", nullable: true),
                    ShouldImport = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Imported = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ValidationErrors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExclusionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PatientWasCreated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PatientFacilityWasCreated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhysicianWasCreated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NursePractitionerWasCreated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HospitalizationWasCreated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAssignments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAssignments",
                schema: "staging");
        }
    }
}
