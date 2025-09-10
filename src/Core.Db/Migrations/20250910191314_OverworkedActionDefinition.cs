using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Db.Migrations
{
    /// <inheritdoc />
    public partial class OverworkedActionDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AllowedSubActions",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "AllowedSubDataDefinitions",
                table: "Tags",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubGridTopTag",
                table: "DataDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagToSelect",
                table: "DataDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "TagUsedInAction",
                table: "ActionDefinitions",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]");

            migrationBuilder.AddColumn<List<string>>(
                name: "TagViaArgument",
                table: "ActionDefinitions",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "ValueViaArgument",
                table: "ActionDefinitions",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedSubActions",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "AllowedSubDataDefinitions",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "SubGridTopTag",
                table: "DataDefinitions");

            migrationBuilder.DropColumn(
                name: "TagToSelect",
                table: "DataDefinitions");

            migrationBuilder.DropColumn(
                name: "TagViaArgument",
                table: "ActionDefinitions");

            migrationBuilder.DropColumn(
                name: "ValueViaArgument",
                table: "ActionDefinitions");

            migrationBuilder.AlterColumn<string[]>(
                name: "TagUsedInAction",
                table: "ActionDefinitions",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0],
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);
        }
    }
}
