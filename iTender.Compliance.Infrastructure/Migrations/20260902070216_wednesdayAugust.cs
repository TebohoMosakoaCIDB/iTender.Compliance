using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class wednesdayAugust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgsaReferralDeadlineDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AgsaReferralEmail",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnforcementUnitEmail",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InstructionLetterResponseWorkingDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReminderAfterWorkingDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RopCheckAfterDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RopRegistrationGraceDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgsaReferralDeadlineDays",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "AgsaReferralEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EnforcementUnitEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "InstructionLetterResponseWorkingDays",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ReminderAfterWorkingDays",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RopCheckAfterDays",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RopRegistrationGraceDays",
                table: "SystemSettings");
        }
    }
}
