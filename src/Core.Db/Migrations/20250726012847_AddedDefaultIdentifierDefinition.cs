using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultIdentifierDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultIdentifierDefinition",
                table: "Tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultIdentifierDefinition",
                table: "Tags");
        }
    }
}
