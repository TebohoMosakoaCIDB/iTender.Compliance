using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContextThreadingFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceFinding_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFinding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComplianceFinding",
                table: "ComplianceFinding");

            migrationBuilder.RenameTable(
                name: "ComplianceFinding",
                newName: "ComplianceFindings");

            migrationBuilder.RenameIndex(
                name: "IX_ComplianceFinding_ComplianceCaseId",
                table: "ComplianceFindings",
                newName: "IX_ComplianceFindings_ComplianceCaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComplianceFindings",
                table: "ComplianceFindings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceFindings_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFindings",
                column: "ComplianceCaseId",
                principalTable: "ComplianceCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceFindings_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFindings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComplianceFindings",
                table: "ComplianceFindings");

            migrationBuilder.RenameTable(
                name: "ComplianceFindings",
                newName: "ComplianceFinding");

            migrationBuilder.RenameIndex(
                name: "IX_ComplianceFindings_ComplianceCaseId",
                table: "ComplianceFinding",
                newName: "IX_ComplianceFinding_ComplianceCaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComplianceFinding",
                table: "ComplianceFinding",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceFinding_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFinding",
                column: "ComplianceCaseId",
                principalTable: "ComplianceCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
