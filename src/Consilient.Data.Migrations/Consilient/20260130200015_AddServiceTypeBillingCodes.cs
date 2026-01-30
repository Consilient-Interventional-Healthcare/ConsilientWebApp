using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
{
    /// <inheritdoc />
    public partial class AddServiceTypeBillingCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceTypeBillingCodes",
                schema: "Billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    BillingCodeId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypeBillingCode", x => x.Id);
                    table.UniqueConstraint("AK_ServiceTypeBillingCodes_ServiceTypeId_BillingCodeId", x => new { x.ServiceTypeId, x.BillingCodeId });
                    table.ForeignKey(
                        name: "FK_ServiceTypeBillingCodes_BillingCodes",
                        column: x => x.BillingCodeId,
                        principalSchema: "Billing",
                        principalTable: "BillingCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceTypeBillingCodes_ServiceTypes",
                        column: x => x.ServiceTypeId,
                        principalSchema: "Clinical",
                        principalTable: "ServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypeBillingCodes_BillingCodeId",
                schema: "Billing",
                table: "ServiceTypeBillingCodes",
                column: "BillingCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypeBillingCodes_ServiceTypeId",
                schema: "Billing",
                table: "ServiceTypeBillingCodes",
                column: "ServiceTypeId");

       }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceTypeBillingCodes",
                schema: "Billing");
        }
    }
}
