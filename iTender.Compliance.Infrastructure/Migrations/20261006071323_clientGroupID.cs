using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTender.Compliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class clientGroupID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnnexureFileName",
                table: "ClientGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnexureFilePath",
                table: "ClientGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGroupId",
                table: "CaseLetters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseLetters_ClientGroupId",
                table: "CaseLetters",
                column: "ClientGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseLetters_ClientGroups_ClientGroupId",
                table: "CaseLetters",
                column: "ClientGroupId",
                principalTable: "ClientGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseLetters_ClientGroups_ClientGroupId",
                table: "CaseLetters");

            migrationBuilder.DropIndex(
                name: "IX_CaseLetters_ClientGroupId",
                table: "CaseLetters");

            migrationBuilder.DropColumn(
                name: "AnnexureFileName",
                table: "ClientGroups");

            migrationBuilder.DropColumn(
                name: "AnnexureFilePath",
                table: "ClientGroups");

            migrationBuilder.DropColumn(
                name: "ClientGroupId",
                table: "CaseLetters");
        }
    }
}
