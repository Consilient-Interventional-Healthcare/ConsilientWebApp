using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Consilient.Data.Migrations.Consilient
{
    /// <inheritdoc />
    public partial class AddUserToProviderAssignmentBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAssignmentBatches_CreatedByUserId",
                schema: "staging",
                table: "ProviderAssignmentBatches",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProviderAssignmentBatches_CreatedByUserId",
                schema: "staging",
                table: "ProviderAssignmentBatches");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "staging",
                table: "ProviderAssignmentBatches");
        }
    }
}
