using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BRSChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitle",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RoundRobinCursor",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DuplicateGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateGroups_TenderSyncs_TenderSyncId",
                        column: x => x.TenderSyncId,
                        principalTable: "TenderSyncs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuplicateGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    MatchedOn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateCandidates_DuplicateGroups_DuplicateGroupId",
                        column: x => x.DuplicateGroupId,
                        principalTable: "DuplicateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuplicateCandidates_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_DuplicateGroupId",
                table: "DuplicateCandidates",
                column: "DuplicateGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_TenderId",
                table: "DuplicateCandidates",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateGroups_Status",
                table: "DuplicateGroups",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateGroups_TenderSyncId",
                table: "DuplicateGroups",
                column: "TenderSyncId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuplicateCandidates");

            migrationBuilder.DropTable(
                name: "DuplicateGroups");

            migrationBuilder.DropColumn(
                name: "NormalizedTitle",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "RoundRobinCursor",
                table: "SystemSettings");
        }
    }
}
