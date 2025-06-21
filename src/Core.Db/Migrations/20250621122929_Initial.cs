using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Core.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    MathJsUri = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionFunction = table.Column<string>(type: "text", nullable: false),
                    TagUsedInAction = table.Column<string[]>(type: "text[]", nullable: false),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionDefinitions_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultipleValues = table.Column<bool>(type: "boolean", nullable: false),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    AutoIncrease = table.Column<bool>(type: "boolean", nullable: false),
                    InitialValue = table.Column<string>(type: "text", nullable: true),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    ActionForCalculated = table.Column<string>(type: "text", nullable: true),
                    CalculateType = table.Column<int>(type: "integer", nullable: true),
                    ConnectionType = table.Column<int>(type: "integer", nullable: true),
                    PathForConnected = table.Column<string>(type: "text", nullable: true),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataDefinitions_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UniqueDefinition = table.Column<bool>(type: "boolean", nullable: false),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Values = table.Column<List<string>>(type: "text[]", nullable: false),
                    CaseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataEntries_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataEntries_DataDefinitions_DataDefinitionId",
                        column: x => x.DataDefinitionId,
                        principalTable: "DataDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataEntryTag",
                columns: table => new
                {
                    DataEntriesId = table.Column<long>(type: "bigint", nullable: false),
                    TagsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntryTag", x => new { x.DataEntriesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_DataEntryTag_DataEntries_DataEntriesId",
                        column: x => x.DataEntriesId,
                        principalTable: "DataEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataEntryTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_CaseId_Name",
                table: "ActionDefinitions",
                columns: new[] { "CaseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_Name",
                table: "Cases",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_TenantId",
                table: "Cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DataDefinitions_CaseId_Name",
                table: "DataDefinitions",
                columns: new[] { "CaseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_CaseId_DataDefinitionId",
                table: "DataEntries",
                columns: new[] { "CaseId", "DataDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_DataDefinitionId",
                table: "DataEntries",
                column: "DataDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntryTag_TagsId",
                table: "DataEntryTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CaseId_Name",
                table: "Tags",
                columns: new[] { "CaseId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionDefinitions");

            migrationBuilder.DropTable(
                name: "DataEntryTag");

            migrationBuilder.DropTable(
                name: "DataEntries");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "DataDefinitions");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
