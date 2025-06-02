using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddedCaseToIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_CaseId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_DataSets_CaseId",
                table: "DataSets");

            migrationBuilder.DropIndex(
                name: "IX_DataSets_Name",
                table: "DataSets");

            migrationBuilder.DropIndex(
                name: "IX_DataDefinitions_CaseId",
                table: "DataDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_DataDefinitions_Name",
                table: "DataDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ActionDefinitions_CaseId",
                table: "ActionDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ActionDefinitions_Name",
                table: "ActionDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CaseId_Name",
                table: "Tags",
                columns: new[] { "CaseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_CaseId_Name",
                table: "DataSets",
                columns: new[] { "CaseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataDefinitions_CaseId_Name",
                table: "DataDefinitions",
                columns: new[] { "CaseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_CaseId_Name",
                table: "ActionDefinitions",
                columns: new[] { "CaseId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_CaseId_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_DataSets_CaseId_Name",
                table: "DataSets");

            migrationBuilder.DropIndex(
                name: "IX_DataDefinitions_CaseId_Name",
                table: "DataDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ActionDefinitions_CaseId_Name",
                table: "ActionDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CaseId",
                table: "Tags",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_CaseId",
                table: "DataSets",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_Name",
                table: "DataSets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataDefinitions_CaseId",
                table: "DataDefinitions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DataDefinitions_Name",
                table: "DataDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_CaseId",
                table: "ActionDefinitions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_Name",
                table: "ActionDefinitions",
                column: "Name",
                unique: true);
        }
    }
}
