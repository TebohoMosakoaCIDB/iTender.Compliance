using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class secondGo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassificationStatus",
                table: "Tenders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassificationStatus",
                table: "Tenders");
        }
    }
}
