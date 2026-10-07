using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastMonthlyReportSent",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyReportHourUtc",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MonthlyReportRecipientEmail",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendMonthlyReportAutomatically",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMonthlyReportSent",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "MonthlyReportHourUtc",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "MonthlyReportRecipientEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SendMonthlyReportAutomatically",
                table: "SystemSettings");
        }
    }
}
