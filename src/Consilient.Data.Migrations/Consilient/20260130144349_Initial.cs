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
                name: "Billing");

            migrationBuilder.EnsureSchema(
                name: "Compensation");

            migrationBuilder.EnsureSchema(
                name: "Clinical");

            migrationBuilder.EnsureSchema(
                name: "staging");

            migrationBuilder.CreateTable(
                name: "BillingCodes",
                schema: "Billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingCodes", x => x.Id);
                    table.UniqueConstraint("AK_BillingCodes_Code", x => x.Code);
                });

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
                    Role = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "HospitalizationStatuses",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationStatus", x => x.Id);
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
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderAssignmentBatchStatuses",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAssignmentBatchStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderTypes",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitEventTypes",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitEventType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderContracts",
                schema: "Compensation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderContracts_Employee",
                        column: x => x.EmployeeID,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProviderContracts_Facility",
                        column: x => x.FacilityId,
                        principalSchema: "Clinical",
                        principalTable: "Facilities",
                        principalColumn: "Id");
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
                    HospitalizationStatusId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId",
                        column: x => x.HospitalizationStatusId,
                        principalSchema: "Clinical",
                        principalTable: "HospitalizationStatuses",
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
                name: "PatientFacilities",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    MRN = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientFacility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientFacilities_Facilities",
                        column: x => x.FacilityId,
                        principalSchema: "Clinical",
                        principalTable: "Facilities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientFacilities_Patients",
                        column: x => x.PatientId,
                        principalSchema: "Clinical",
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProviderAssignmentBatches",
                schema: "staging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAssignmentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderAssignmentBatches_ProviderAssignmentBatchStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "staging",
                        principalTable: "ProviderAssignmentBatchStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProviderTypeId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Compensation",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Providers_ProviderTypes_ProviderTypeId",
                        column: x => x.ProviderTypeId,
                        principalSchema: "Clinical",
                        principalTable: "ProviderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "HospitalizationStatusHistories",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HospitalizationId = table.Column<int>(type: "int", nullable: false),
                    NewStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalizationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId",
                        column: x => x.NewStatusId,
                        principalSchema: "Clinical",
                        principalTable: "HospitalizationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HospitalizationStatusHistories_Hospitalizations_HospitalizationId",
                        column: x => x.HospitalizationId,
                        principalSchema: "Clinical",
                        principalTable: "Hospitalizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "VisitAttendants",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitAttendant", x => x.Id);
                    table.UniqueConstraint("AK_VisitAttendants_VisitId_ProviderId", x => new { x.VisitId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_VisitAttendants_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "Clinical",
                        principalTable: "Providers",
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

            migrationBuilder.CreateTable(
                name: "VisitEvents",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    EventOccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitEvents_VisitEventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalSchema: "Clinical",
                        principalTable: "VisitEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitEvents_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "Clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitServiceBillings",
                schema: "Billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    BillingCodeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitServiceBilling", x => x.Id);
                    table.UniqueConstraint("AK_VisitServiceBillings_VisitId_ServiceTypeId_BillingCodeId", x => new { x.VisitId, x.ServiceTypeId, x.BillingCodeId });
                    table.ForeignKey(
                        name: "FK_VisitServiceBillings_BillingCodes_BillingCodeId",
                        column: x => x.BillingCodeId,
                        principalSchema: "Billing",
                        principalTable: "BillingCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitServiceBillings_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitServiceBillings_Visits_VisitId",
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
                name: "IX_Hospitalizations_HospitalizationStatusId",
                schema: "Clinical",
                table: "Hospitalizations",
                column: "HospitalizationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Hospitalizations_PatientId",
                schema: "Clinical",
                table: "Hospitalizations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatuses_Code",
                schema: "Clinical",
                table: "HospitalizationStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatusHistories_ChangedAt",
                schema: "Clinical",
                table: "HospitalizationStatusHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatusHistories_ChangedByUserId",
                schema: "Clinical",
                table: "HospitalizationStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatusHistories_HospitalizationId",
                schema: "Clinical",
                table: "HospitalizationStatusHistories",
                column: "HospitalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatusHistories_NewStatusId",
                schema: "Clinical",
                table: "HospitalizationStatusHistories",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientFacilities_FacilityId_MRN",
                schema: "Clinical",
                table: "PatientFacilities",
                columns: new[] { "FacilityId", "MRN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientFacilities_PatientId",
                schema: "Clinical",
                table: "PatientFacilities",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_CreatedByUserId",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_FacilityId_Date",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                columns: new[] { "FacilityId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_Status",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatchStatuses_Code",
                schema: "staging",
                table: "ProviderAssignmentBatchStatuses",
                column: "Code",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_ProviderContracts_EmployeeID",
                schema: "Compensation",
                table: "ProviderContracts",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderContracts_FacilityId",
                schema: "Compensation",
                table: "ProviderContracts",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_EmployeeId",
                schema: "Clinical",
                table: "Providers",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderTypeId",
                schema: "Clinical",
                table: "Providers",
                column: "ProviderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderTypes_Code",
                schema: "Clinical",
                table: "ProviderTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_Code",
                schema: "Clinical",
                table: "ServiceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitAttendants_ProviderId",
                schema: "Clinical",
                table: "VisitAttendants",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitAttendants_VisitId",
                schema: "Clinical",
                table: "VisitAttendants",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitEvents_EnteredByUserId",
                schema: "Clinical",
                table: "VisitEvents",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitEvents_EventOccurredAt",
                schema: "Clinical",
                table: "VisitEvents",
                column: "EventOccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_VisitEvents_EventTypeId",
                schema: "Clinical",
                table: "VisitEvents",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitEvents_VisitId",
                schema: "Clinical",
                table: "VisitEvents",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitEventTypes_Code",
                schema: "Clinical",
                table: "VisitEventTypes",
                column: "Code",
                unique: true);

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
                name: "IX_VisitServiceBillings_BillingCodeId",
                schema: "Billing",
                table: "VisitServiceBillings",
                column: "BillingCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitServiceBillings_ServiceTypeId",
                schema: "Billing",
                table: "VisitServiceBillings",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitServiceBillings_VisitId",
                schema: "Billing",
                table: "VisitServiceBillings",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HospitalizationInsurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "HospitalizationStatusHistories",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "PatientFacilities",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "ProviderAssignments",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "ProviderContracts",
                schema: "Compensation");

            migrationBuilder.DropTable(
                name: "VisitAttendants",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitEvents",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitServiceBillings",
                schema: "Billing");

            migrationBuilder.DropTable(
                name: "Insurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "ProviderAssignmentBatches",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "Providers",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitEventTypes",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "BillingCodes",
                schema: "Billing");

            migrationBuilder.DropTable(
                name: "ServiceTypes",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Visits",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "ProviderAssignmentBatchStatuses",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Compensation");

            migrationBuilder.DropTable(
                name: "ProviderTypes",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Hospitalizations",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Facilities",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "HospitalizationStatuses",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Patients",
                schema: "Clinical");
        }
    }
}
