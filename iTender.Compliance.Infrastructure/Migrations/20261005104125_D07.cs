using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class D07 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstructionLetterReminderAfterWorkingDays",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructionLetterReminderAfterWorkingDays",
                table: "SystemSettings");
        }
    }
}
