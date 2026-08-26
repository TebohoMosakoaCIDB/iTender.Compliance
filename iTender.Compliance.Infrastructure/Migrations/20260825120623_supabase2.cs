using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class supabase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceFindings_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFindings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComplianceFindings",
                table: "ComplianceFindings");

            migrationBuilder.DropColumn(
                name: "InstructionalLetterResponseHours",
                table: "SystemSettings");

            migrationBuilder.RenameTable(
                name: "ComplianceFindings",
                newName: "ComplianceFinding");

            migrationBuilder.RenameColumn(
                name: "LastInstructionalLetterNumber",
                table: "SystemSettings",
                newName: "OpenTenderResponseHours");

            migrationBuilder.RenameColumn(
                name: "LastContraventionNoticeNumber",
                table: "SystemSettings",
                newName: "ClosedTenderResponseDays");

            migrationBuilder.RenameIndex(
                name: "IX_ComplianceFindings_ComplianceCaseId",
                table: "ComplianceFinding",
                newName: "IX_ComplianceFinding_ComplianceCaseId");

            migrationBuilder.AddColumn<bool>(
                name: "RequireManagerApproval",
                table: "SystemSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "Agents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComplianceFinding",
                table: "ComplianceFinding",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AGSAReferrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    AgsaResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgsaResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AGSAReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AGSAReferrals_ComplianceCases_ComplianceCaseId",
                        column: x => x.ComplianceCaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseObjections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseLetterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Decision = table.Column<int>(type: "integer", nullable: true),
                    ManagerNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseObjections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseObjections_CaseLetters_CaseLetterId",
                        column: x => x.CaseLetterId,
                        principalTable: "CaseLetters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseObjections_ComplianceCases_ComplianceCaseId",
                        column: x => x.ComplianceCaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AGSAReferrals_ComplianceCaseId",
                table: "AGSAReferrals",
                column: "ComplianceCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseObjections_CaseLetterId",
                table: "CaseObjections",
                column: "CaseLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseObjections_ComplianceCaseId",
                table: "CaseObjections",
                column: "ComplianceCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceFinding_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFinding",
                column: "ComplianceCaseId",
                principalTable: "ComplianceCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceFinding_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFinding");

            migrationBuilder.DropTable(
                name: "AGSAReferrals");

            migrationBuilder.DropTable(
                name: "CaseObjections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComplianceFinding",
                table: "ComplianceFinding");

            migrationBuilder.DropColumn(
                name: "RequireManagerApproval",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "Agents");

            migrationBuilder.RenameTable(
                name: "ComplianceFinding",
                newName: "ComplianceFindings");

            migrationBuilder.RenameColumn(
                name: "OpenTenderResponseHours",
                table: "SystemSettings",
                newName: "LastInstructionalLetterNumber");

            migrationBuilder.RenameColumn(
                name: "ClosedTenderResponseDays",
                table: "SystemSettings",
                newName: "LastContraventionNoticeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ComplianceFinding_ComplianceCaseId",
                table: "ComplianceFindings",
                newName: "IX_ComplianceFindings_ComplianceCaseId");

            migrationBuilder.AddColumn<int>(
                name: "InstructionalLetterResponseHours",
                table: "SystemSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComplianceFindings",
                table: "ComplianceFindings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceFindings_ComplianceCases_ComplianceCaseId",
                table: "ComplianceFindings",
                column: "ComplianceCaseId",
                principalTable: "ComplianceCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
