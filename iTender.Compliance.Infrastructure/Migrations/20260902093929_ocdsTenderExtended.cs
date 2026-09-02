using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ocdsTenderExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryLocation",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ocid",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementCategory",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementMethod",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementMethodDetails",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocumentUrl",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialConditions",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenderStatus",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryLocation",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "Ocid",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ProcurementCategory",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ProcurementMethod",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ProcurementMethodDetails",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "SourceDocumentUrl",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "SpecialConditions",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "TenderStatus",
                table: "Tenders");
        }
    }
}
