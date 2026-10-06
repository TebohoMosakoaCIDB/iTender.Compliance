using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class manualTenderClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClassificationOverridden",
                table: "Tenders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClassificationReviewNotes",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassificationReviewedByUserId",
                table: "Tenders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClassificationReviewedOn",
                table: "Tenders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassificationOverridden",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ClassificationReviewNotes",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ClassificationReviewedByUserId",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "ClassificationReviewedOn",
                table: "Tenders");
        }
    }
}
