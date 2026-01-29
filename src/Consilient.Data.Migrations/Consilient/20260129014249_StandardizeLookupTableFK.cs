using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
{
    /// <inheritdoc />
    public partial class StandardizeLookupTableFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExclusionReason",
                schema: "staging",
                table: "ProviderAssignments");

            // Note: DropPrimaryKey, DropColumn Color, and RenameColumn Description are handled in raw SQL below

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "Clinical",
                table: "Providers",
                newName: "ProviderTypeId");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                newName: "StatusId");

            // Drop FK constraints that reference the lookup tables we're recreating
            migrationBuilder.Sql(@"
                -- Drop FK constraints referencing VisitEventTypes
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_VisitEvents_VisitEventTypes_EventTypeId')
                    ALTER TABLE [Clinical].[VisitEvents] DROP CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId];

                -- Drop FK constraints referencing ServiceTypes
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_VisitServiceBillings_ServiceTypes_ServiceTypeId')
                    ALTER TABLE [Billing].[VisitServiceBillings] DROP CONSTRAINT [FK_VisitServiceBillings_ServiceTypes_ServiceTypeId];

                -- Drop FK constraints referencing HospitalizationStatuses
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId')
                    ALTER TABLE [Clinical].[Hospitalizations] DROP CONSTRAINT [FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId')
                    ALTER TABLE [Clinical].[HospitalizationStatusHistories] DROP CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId')
                    ALTER TABLE [staging].[ProviderAssignments] DROP CONSTRAINT [FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId];
            ");

            // Remove IDENTITY from VisitEventTypes.Id (SQL Server requires drop/recreate)
            migrationBuilder.Sql(@"
                -- VisitEventTypes: Remove IDENTITY, add DisplayOrder
                SELECT [Id], [Code], [Name], [CreatedAtUtc], [UpdatedAtUtc] INTO #VisitEventTypes_Backup FROM [Clinical].[VisitEventTypes];
                DROP TABLE [Clinical].[VisitEventTypes];
                CREATE TABLE [Clinical].[VisitEventTypes] (
                    [Id] int NOT NULL,
                    [Code] nvarchar(50) NOT NULL,
                    [Name] nvarchar(100) NOT NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 0,
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [UpdatedAtUtc] datetime2 NOT NULL,
                    [RowVersion] rowversion NOT NULL,
                    CONSTRAINT [PK_VisitEventType] PRIMARY KEY ([Id])
                );
                INSERT INTO [Clinical].[VisitEventTypes] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                SELECT [Id], [Code], [Name], 0, [CreatedAtUtc], [UpdatedAtUtc] FROM #VisitEventTypes_Backup;
                DROP TABLE #VisitEventTypes_Backup;
            ");

            // Remove IDENTITY from ServiceTypes.Id
            migrationBuilder.Sql(@"
                -- ServiceTypes: Remove IDENTITY, rename Description to Name, add Code and DisplayOrder
                SELECT [Id], [Description], [CreatedAtUtc], [UpdatedAtUtc] INTO #ServiceTypes_Backup FROM [Clinical].[ServiceTypes];
                DROP TABLE [Clinical].[ServiceTypes];
                CREATE TABLE [Clinical].[ServiceTypes] (
                    [Id] int NOT NULL,
                    [Code] nvarchar(50) NOT NULL DEFAULT '',
                    [Name] nvarchar(100) NOT NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 0,
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [UpdatedAtUtc] datetime2 NOT NULL,
                    [RowVersion] rowversion NOT NULL,
                    CONSTRAINT [PK_ServiceType] PRIMARY KEY ([Id])
                );
                INSERT INTO [Clinical].[ServiceTypes] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                SELECT [Id], REPLACE([Description], ' ', ''), [Description], 0, [CreatedAtUtc], [UpdatedAtUtc] FROM #ServiceTypes_Backup;
                DROP TABLE #ServiceTypes_Backup;
            ");

            // Remove IDENTITY from HospitalizationStatuses.Id
            migrationBuilder.Sql(@"
                -- HospitalizationStatuses: Remove IDENTITY, drop Color
                SELECT [Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc] INTO #HospitalizationStatuses_Backup FROM [Clinical].[HospitalizationStatuses];
                DROP TABLE [Clinical].[HospitalizationStatuses];
                CREATE TABLE [Clinical].[HospitalizationStatuses] (
                    [Id] int NOT NULL,
                    [Code] nvarchar(50) NOT NULL,
                    [Name] nvarchar(100) NOT NULL,
                    [DisplayOrder] int NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [UpdatedAtUtc] datetime2 NOT NULL,
                    [RowVersion] rowversion NOT NULL,
                    CONSTRAINT [PK_HospitalizationStatus] PRIMARY KEY ([Id])
                );
                INSERT INTO [Clinical].[HospitalizationStatuses] ([Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc])
                SELECT [Id], [Code], [Name], [DisplayOrder], [CreatedAtUtc], [UpdatedAtUtc] FROM #HospitalizationStatuses_Backup;
                DROP TABLE #HospitalizationStatuses_Backup;
            ");

            // Recreate FK constraints for lookup tables
            migrationBuilder.Sql(@"
                -- Recreate FK constraints for VisitEventTypes
                ALTER TABLE [Clinical].[VisitEvents] ADD CONSTRAINT [FK_VisitEvents_VisitEventTypes_EventTypeId]
                    FOREIGN KEY ([EventTypeId]) REFERENCES [Clinical].[VisitEventTypes] ([Id]) ON DELETE NO ACTION;

                -- Recreate FK constraints for ServiceTypes
                ALTER TABLE [Billing].[VisitServiceBillings] ADD CONSTRAINT [FK_VisitServiceBillings_ServiceTypes_ServiceTypeId]
                    FOREIGN KEY ([ServiceTypeId]) REFERENCES [Clinical].[ServiceTypes] ([Id]) ON DELETE NO ACTION;

                -- Recreate FK constraints for HospitalizationStatuses
                ALTER TABLE [Clinical].[Hospitalizations] ADD CONSTRAINT [FK_Hospitalizations_HospitalizationStatuses_HospitalizationStatusId]
                    FOREIGN KEY ([HospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
                ALTER TABLE [Clinical].[HospitalizationStatusHistories] ADD CONSTRAINT [FK_HospitalizationStatusHistories_HospitalizationStatuses_NewStatusId]
                    FOREIGN KEY ([NewStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
                ALTER TABLE [staging].[ProviderAssignments] ADD CONSTRAINT [FK_ProviderAssignments_HospitalizationStatuses_ResolvedHospitalizationStatusId]
                    FOREIGN KEY ([ResolvedHospitalizationStatusId]) REFERENCES [Clinical].[HospitalizationStatuses] ([Id]) ON DELETE NO ACTION;
            ");

            // Note: AddPrimaryKey for ServiceTypes is handled in raw SQL above

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

            // Seed lookup tables before creating FK constraints
            migrationBuilder.Sql(@"
                -- Seed ProviderTypes
                INSERT INTO [Clinical].[ProviderTypes] ([Id], [Code], [Name], [DisplayOrder])
                VALUES
                (0, N'MD', N'Physician', 1),
                (1, N'NP', N'Nurse Practitioner', 2);

                -- Seed ProviderAssignmentBatchStatuses
                INSERT INTO [staging].[ProviderAssignmentBatchStatuses] ([Id], [Code], [Name], [DisplayOrder])
                VALUES
                (0, N'Pending', N'Pending', 1),
                (1, N'Imported', N'Imported', 2),
                (2, N'Resolved', N'Resolved', 3),
                (3, N'Processed', N'Processed', 4);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_Code",
                schema: "Clinical",
                table: "ServiceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ProviderTypeId",
                schema: "Clinical",
                table: "Providers",
                column: "ProviderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalizationStatuses_Code",
                schema: "Clinical",
                table: "HospitalizationStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatchStatuses_Code",
                schema: "staging",
                table: "ProviderAssignmentBatchStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderTypes_Code",
                schema: "Clinical",
                table: "ProviderTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderAssignmentBatches_ProviderAssignmentBatchStatuses_StatusId",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                column: "StatusId",
                principalSchema: "staging",
                principalTable: "ProviderAssignmentBatchStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_ProviderTypes_ProviderTypeId",
                schema: "Clinical",
                table: "Providers",
                column: "ProviderTypeId",
                principalSchema: "Clinical",
                principalTable: "ProviderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderAssignmentBatches_ProviderAssignmentBatchStatuses_StatusId",
                schema: "staging",
                table: "ProviderAssignmentBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_ProviderTypes_ProviderTypeId",
                schema: "Clinical",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "ProviderAssignmentBatchStatuses",
                schema: "staging");

            migrationBuilder.DropTable(
                name: "ProviderTypes",
                schema: "Clinical");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceType",
                schema: "Clinical",
                table: "ServiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTypes_Code",
                schema: "Clinical",
                table: "ServiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ProviderTypeId",
                schema: "Clinical",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_HospitalizationStatuses_Code",
                schema: "Clinical",
                table: "HospitalizationStatuses");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "Clinical",
                table: "VisitEventTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Clinical",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "Clinical",
                table: "ServiceTypes");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Clinical",
                table: "ServiceTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ProviderTypeId",
                schema: "Clinical",
                table: "Providers",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Clinical",
                table: "VisitEventTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Clinical",
                table: "ServiceTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ExclusionReason",
                schema: "staging",
                table: "ProviderAssignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Clinical",
                table: "HospitalizationStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "Clinical",
                table: "HospitalizationStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceTypes",
                schema: "Clinical",
                table: "ServiceTypes",
                column: "Id");
        }
    }
}
