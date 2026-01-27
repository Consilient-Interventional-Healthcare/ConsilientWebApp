using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient;

/// <inheritdoc />
public partial class ServiceTypeAdjustments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Visits_ServiceTypes_ServiceTypeId",
            schema: "Clinical",
            table: "Visits");

        migrationBuilder.DropIndex(
            name: "IX_Visits_ServiceTypeId",
            schema: "Clinical",
            table: "Visits");

        migrationBuilder.DropColumn(
            name: "ServiceTypeId",
            schema: "Clinical",
            table: "Visits");

        migrationBuilder.DropColumn(
            name: "CPTCode",
            schema: "Clinical",
            table: "ServiceTypes");

        migrationBuilder.EnsureSchema(
            name: "Billing");

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
            name: "VisitServiceBillings",
            schema: "Billing");

        migrationBuilder.DropTable(
            name: "BillingCodes",
            schema: "Billing");

        migrationBuilder.AddColumn<int>(
            name: "ServiceTypeId",
            schema: "Clinical",
            table: "Visits",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "CPTCode",
            schema: "Clinical",
            table: "ServiceTypes",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Visits_ServiceTypeId",
            schema: "Clinical",
            table: "Visits",
            column: "ServiceTypeId");

        migrationBuilder.AddForeignKey(
            name: "FK_Visits_ServiceTypes_ServiceTypeId",
            schema: "Clinical",
            table: "Visits",
            column: "ServiceTypeId",
            principalSchema: "Clinical",
            principalTable: "ServiceTypes",
            principalColumn: "Id");
    }
}
