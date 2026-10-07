using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuarterlyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastQuarterlyReportSent",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuarterlyReportRecipientEmail",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendQuarterlyReportAutomatically",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastQuarterlyReportSent",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "QuarterlyReportRecipientEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SendQuarterlyReportAutomatically",
                table: "SystemSettings");
        }
    }
}
