using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleExtension = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    IsProvider = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAdministrator = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CanApproveVisits = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insurances",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhysicianIncluded = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsContracted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MRN = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.UniqueConstraint("AK_Patients_MRN", x => x.MRN);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CPTCode = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.Id);
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
                    PsychEvaluation = table.Column<bool>(type: "bit", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hospitalizations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitsStaging",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateServiced = table.Column<DateOnly>(type: "date", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    AdmissionNumber = table.Column<int>(type: "int", nullable: true),
                    InsuranceId = table.Column<int>(type: "int", nullable: true),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: true),
                    PhysicianEmployeeId = table.Column<int>(type: "int", nullable: false),
                    NursePractitionerEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ScribeEmployeeId = table.Column<int>(type: "int", nullable: true),
                    NursePractitionerApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhysicianApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhysicianApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhysicianApprovedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedToMainTable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CosigningPhysicianEmployeeId = table.Column<int>(type: "int", nullable: true),
                    IsScribeServiceOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitsStaging", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitsStaging_CosignPhysicianEmployee",
                        column: x => x.CosigningPhysicianEmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_Facility",
                        column: x => x.FacilityId,
                        principalSchema: "Clinical",
                        principalTable: "Facilities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_Insurance",
                        column: x => x.InsuranceId,
                        principalSchema: "Clinical",
                        principalTable: "Insurances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_NursePractitioner",
                        column: x => x.NursePractitionerEmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_Patient",
                        column: x => x.PatientId,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_Physician",
                        column: x => x.PhysicianEmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_Scribe",
                        column: x => x.ScribeEmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitsStaging_ServiceType",
                        column: x => x.ServiceTypeId,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HospitalizationInsurances",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HospitalizationId = table.Column<int>(type: "int", nullable: false),
                    InsuranceId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationInsurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalizationInsurances_Hospitalizations",
                        column: x => x.HospitalizationId,
                        principalSchema: "Clinical",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HospitalizationInsurances_Insurances",
                        column: x => x.InsuranceId,
                        principalSchema: "Clinical",
                        principalTable: "Insurances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateServiced = table.Column<DateOnly>(type: "date", nullable: false),
                    HospitalizationId = table.Column<int>(type: "int", nullable: false),
                    IsScribeServiceOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Bed = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "Clinical",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visits_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VisitAttendants",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitAttendant", x => x.Id);
                    table.UniqueConstraint("AK_VisitAttendants_VisitId_EmployeeId", x => new { x.VisitId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_VisitAttendants_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitAttendants_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "Clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationInsurances_HospitalizationId",
                schema: "Clinical",
                table: "HospitalizationInsurances",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationInsurances_InsuranceId",
                schema: "Clinical",
                table: "HospitalizationInsurances",
                column: "InsuranceId");

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
                name: "IX_VisitAttendants_EmployeeId",
                schema: "Clinical",
                table: "VisitAttendants",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitAttendants_VisitId",
                schema: "Clinical",
                table: "VisitAttendants",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DateServiced",
                schema: "Clinical",
                table: "Visits",
                column: "DateServiced");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_HospitalizationId",
                schema: "Clinical",
                table: "Visits",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ServiceTypeId",
                schema: "Clinical",
                table: "Visits",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_CosigningPhysicianEmployeeId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "CosigningPhysicianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_FacilityId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_InsuranceId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_NursePractitionerEmployeeId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "NursePractitionerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_PatientId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_PhysicianEmployeeId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "PhysicianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_ScribeEmployeeId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "ScribeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStaging_ServiceTypeId",
                schema: "Clinical",
                table: "VisitsStaging",
                column: "ServiceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HospitalizationInsurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitAttendants",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitsStaging",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Visits",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Compensation");

            migrationBuilder.DropTable(
                name: "Insurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Hospitalizations",
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
