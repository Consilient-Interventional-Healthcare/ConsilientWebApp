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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                name: "VisitEventTypes",
                schema: "Clinical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitEventType", x => x.Id);
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
                    Type = table.Column<int>(type: "int", nullable: false),
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
                name: "IX_Visits_ServiceTypeId",
                schema: "Clinical",
                table: "Visits",
                column: "ServiceTypeId");
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
                name: "ProviderContracts",
                schema: "Compensation");

            migrationBuilder.DropTable(
                name: "VisitAttendants",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitEvents",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Insurances",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Providers",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "VisitEventTypes",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Visits",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Compensation");

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
                name: "HospitalizationStatuses",
                schema: "Clinical");

            migrationBuilder.DropTable(
                name: "Patients",
                schema: "Clinical");
        }
    }
}
