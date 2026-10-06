using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class clientGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdvertised",
                table: "Tenders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmployerName",
                table: "Tenders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClientGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NormalizedEmployerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CcEmails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ConsolidatedLetterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGroups_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientGroups_CaseLetters_ConsolidatedLetterId",
                        column: x => x.ConsolidatedLetterId,
                        principalTable: "CaseLetters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientGroupCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplianceCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGroupCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGroupCases_ClientGroups_ClientGroupId",
                        column: x => x.ClientGroupId,
                        principalTable: "ClientGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientGroupCases_ComplianceCases_ComplianceCaseId",
                        column: x => x.ComplianceCaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroupCases_ClientGroupId_ComplianceCaseId",
                table: "ClientGroupCases",
                columns: new[] { "ClientGroupId", "ComplianceCaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroupCases_ComplianceCaseId",
                table: "ClientGroupCases",
                column: "ComplianceCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_AgentId",
                table: "ClientGroups",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_ConsolidatedLetterId",
                table: "ClientGroups",
                column: "ConsolidatedLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_NormalizedEmployerName",
                table: "ClientGroups",
                column: "NormalizedEmployerName");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_Status",
                table: "ClientGroups",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientGroupCases");

            migrationBuilder.DropTable(
                name: "ClientGroups");

            migrationBuilder.DropColumn(
                name: "IsAdvertised",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "NormalizedEmployerName",
                table: "Tenders");
        }
    }
}
