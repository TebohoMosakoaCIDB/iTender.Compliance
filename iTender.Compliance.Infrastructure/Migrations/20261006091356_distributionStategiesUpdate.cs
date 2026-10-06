using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class distributionStategiesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaximumOpenCases",
                table: "Agents",
                newName: "MaximumOpenGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaximumOpenGroups",
                table: "Agents",
                newName: "MaximumOpenCases");
        }
    }
}
