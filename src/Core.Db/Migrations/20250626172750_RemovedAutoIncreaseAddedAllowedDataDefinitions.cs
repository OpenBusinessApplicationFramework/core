using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Db.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAutoIncreaseAddedAllowedDataDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataEntries_CaseId_DataDefinitionId",
                table: "DataEntries");

            migrationBuilder.DropColumn(
                name: "AutoIncrease",
                table: "DataDefinitions");

            migrationBuilder.AddColumn<List<string>>(
                name: "AllowedDataDefinitions",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_CaseId",
                table: "DataEntries",
                column: "CaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataEntries_CaseId",
                table: "DataEntries");

            migrationBuilder.DropColumn(
                name: "AllowedDataDefinitions",
                table: "Tags");

            migrationBuilder.AddColumn<bool>(
                name: "AutoIncrease",
                table: "DataDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_CaseId_DataDefinitionId",
                table: "DataEntries",
                columns: new[] { "CaseId", "DataDefinitionId" },
                unique: true);
        }
    }
}
