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
                name: "ProviderAssignmentBatches",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAssignmentBatches", x => x.Id);
                });

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
                    ResolvedHospitalizationStatusId = table.Column<int>(type: "int", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId",
                        column: x => x.ResolvedHospitalizationStatusId,
                        principalSchema: "Clinical",
                        principalTable: "HospitalizationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_Hospitalizations_ResolvedHospitalizationId",
                        column: x => x.ResolvedHospitalizationId,
                        principalSchema: "Clinical",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_Patients_ResolvedPatientId",
                        column: x => x.ResolvedPatientId,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_ProviderAssignmentBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "staging",
                        principalTable: "ProviderAssignmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_Providers_ResolvedNursePractitionerId",
                        column: x => x.ResolvedNursePractitionerId,
                        principalSchema: "Clinical",
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_Providers_ResolvedPhysicianId",
                        column: x => x.ResolvedPhysicianId,
                        principalSchema: "Clinical",
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderAssignments_Visits_ResolvedVisitId",
                        column: x => x.ResolvedVisitId,
                        principalSchema: "Clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_FacilityId_Date",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                columns: new[] { "FacilityId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_Status",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_BatchId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedHospitalizationId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedHospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedHospitalizationStatusId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedHospitalizationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedNursePractitionerId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedNursePractitionerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedPatientId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedPhysicianId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedPhysicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignments_ResolvedVisitId",
                schema: "staging",
                table: "ProviderAssignments",
                column: "ResolvedVisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAssignments",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "ProviderAssignmentBatches",
                schema: "staging");
        }
    }
}
