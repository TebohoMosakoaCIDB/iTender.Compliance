using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlaWarningDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlaWarningDays",
                table: "SystemSettings");
        }
    }
}
