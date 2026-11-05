using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class clientRespomse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplianceCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseLetterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InformationRequestedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InformationRequestDueOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientResponses_CaseLetters_CaseLetterId",
                        column: x => x.CaseLetterId,
                        principalTable: "CaseLetters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientResponses_ClientResponses_PreviousResponseId",
                        column: x => x.PreviousResponseId,
                        principalTable: "ClientResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientResponses_ComplianceCases_ComplianceCaseId",
                        column: x => x.ComplianceCaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientResponseDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientResponseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientResponseDocuments_ClientResponses_ClientResponseId",
                        column: x => x.ClientResponseId,
                        principalTable: "ClientResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponseDocuments_ClientResponseId",
                table: "ClientResponseDocuments",
                column: "ClientResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponses_CaseLetterId",
                table: "ClientResponses",
                column: "CaseLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponses_ComplianceCaseId",
                table: "ClientResponses",
                column: "ComplianceCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponses_PreviousResponseId",
                table: "ClientResponses",
                column: "PreviousResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponses_Status",
                table: "ClientResponses",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientResponseDocuments");

            migrationBuilder.DropTable(
                name: "ClientResponses");
        }
    }
}
