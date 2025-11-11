using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Compensation");

            migrationBuilder.EnsureSchema(
                name: "Clinical");

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "Compensation",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TitleExtension = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    IsProvider = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdministrator = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CanApproveVisits = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                schema: "Clinical",
                columns: table => new
                {
                    FacilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FacilityAbbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.FacilityID);
                });

            migrationBuilder.CreateTable(
                name: "Insurances",
                schema: "Clinical",
                columns: table => new
                {
                    InsuranceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsuranceCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InsuranceDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhysicianIncluded = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsContracted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CodeAndDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.InsuranceID);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                schema: "Clinical",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientMRN = table.Column<int>(type: "int", nullable: false),
                    PatientFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PatientLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PatientBirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PatientFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientID);
                    table.UniqueConstraint("AK_Patients_PatientMrn", x => x.PatientMRN);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                schema: "Clinical",
                columns: table => new
                {
                    ServiceTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CPTCode = table.Column<int>(type: "int", nullable: true),
                    CodeAndDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.ServiceTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Hospitalizations",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    AdmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DischargeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitalization", x => x.Id);
                    table.UniqueConstraint("AK_Hospitalizations_CaseId", x => x.CaseId);
                    table.UniqueConstraint("AK_Hospitalizations_CaseId_PatientId", x => new { x.CaseId, x.PatientId });
                    table.ForeignKey(
                        name: "FK_Hospitalizations_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalSchema: "Clinical",
                        principalTable: "Facilities",
                        principalColumn: "FacilityID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hospitalizations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientVisits_Staging",
                schema: "Clinical",
                columns: table => new
                {
                    PatientVisit_StagingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateServiced = table.Column<DateOnly>(type: "date", nullable: false),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    FacilityID = table.Column<int>(type: "int", nullable: false),
                    AdmissionNumber = table.Column<int>(type: "int", nullable: true),
                    InsuranceID = table.Column<int>(type: "int", nullable: true),
                    ServiceTypeID = table.Column<int>(type: "int", nullable: true),
                    PhysicianEmployeeID = table.Column<int>(type: "int", nullable: false),
                    NursePractitionerEmployeeID = table.Column<int>(type: "int", nullable: true),
                    ScribeEmployeeID = table.Column<int>(type: "int", nullable: true),
                    NursePractitionerApproved = table.Column<bool>(type: "bit", nullable: false),
                    PhysicianApproved = table.Column<bool>(type: "bit", nullable: false),
                    PhysicianApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhysicianApprovedDateTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    AddedToMainTable = table.Column<bool>(type: "bit", nullable: false),
                    CosigningPhysicianEmployeeID = table.Column<int>(type: "int", nullable: true),
                    IsScribeServiceOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientVisits_Staging", x => x.PatientVisit_StagingID);
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_CosignPhysicianEmployee",
                        column: x => x.CosigningPhysicianEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_Facility",
                        column: x => x.FacilityID,
                        principalSchema: "Clinical",
                        principalTable: "Facilities",
                        principalColumn: "FacilityID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_Insurance",
                        column: x => x.InsuranceID,
                        principalSchema: "Clinical",
                        principalTable: "Insurances",
                        principalColumn: "InsuranceID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_NursePractitioner",
                        column: x => x.NursePractitionerEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_Patient",
                        column: x => x.PatientID,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_Physician",
                        column: x => x.PhysicianEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_Scribe",
                        column: x => x.ScribeEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Staging_ServiceType",
                        column: x => x.ServiceTypeID,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeID");
                });

            migrationBuilder.CreateTable(
                name: "PatientVisits",
                schema: "Clinical",
                columns: table => new
                {
                    PatientVisitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosigningPhysicianEmployeeID = table.Column<int>(type: "int", nullable: true),
                    DateServiced = table.Column<DateOnly>(type: "date", nullable: false),
                    HospitalizationID = table.Column<int>(type: "int", nullable: false),
                    InsuranceID = table.Column<int>(type: "int", nullable: true),
                    IsScribeServiceOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsSupervising = table.Column<int>(type: "int", nullable: false),
                    NursePractitionerEmployeeID = table.Column<int>(type: "int", nullable: true),
                    PhysicianEmployeeID = table.Column<int>(type: "int", nullable: false),
                    ScribeEmployeeID = table.Column<int>(type: "int", nullable: true),
                    ServiceTypeID = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientVisits", x => x.PatientVisitID);
                    table.ForeignKey(
                        name: "FK_PatientVisits_CosignPhysicianEmployee",
                        column: x => x.CosigningPhysicianEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Hospitalizations",
                        column: x => x.HospitalizationID,
                        principalSchema: "Clinical",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Insurances",
                        column: x => x.InsuranceID,
                        principalSchema: "Clinical",
                        principalTable: "Insurances",
                        principalColumn: "InsuranceID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_NursePractitioner",
                        column: x => x.NursePractitionerEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Physician",
                        column: x => x.PhysicianEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_Scribe",
                        column: x => x.ScribeEmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_PatientVisits_ServiceType",
                        column: x => x.ServiceTypeID,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hospitalizations_FacilityId",
                schema: "Clinical",
                table: "Hospitalizations",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Hospitalizations_PatientId",
                schema: "Clinical",
                table: "Hospitalizations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_CosigningPhysicianEmployeeID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "CosigningPhysicianEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_HospitalizationID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "HospitalizationID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_InsuranceID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "InsuranceID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_NursePractitionerEmployeeID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "NursePractitionerEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_PhysicianEmployeeID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "PhysicianEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_ScribeEmployeeID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "ScribeEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_ServiceTypeID",
                schema: "Clinical",
                table: "PatientVisits",
                column: "ServiceTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_CosigningPhysicianEmployeeID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "CosigningPhysicianEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_FacilityID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_InsuranceID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "InsuranceID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_NursePractitionerEmployeeID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "NursePractitionerEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_PatientID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_PhysicianEmployeeID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "PhysicianEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_ScribeEmployeeID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "ScribeEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVisits_Staging_ServiceTypeID",
                schema: "Clinical",
                table: "PatientVisits_Staging",
                column: "ServiceTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientVisits",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "PatientVisits_Staging",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Hospitalizations",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Compensation");

            migrationBuilder.DropTable(
                name: "Insurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "ServiceTypes",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Facilities",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Patients",
                schema: "Clinical");
        }
    }
}
