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
                    DataSetUsedInAction = table.Column<string[]>(type: "text[]", nullable: false),
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
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_Cases_CaseId",
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
                    DataSetId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_DataEntries_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DataEntryId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Tags_DataEntries_DataEntryId",
                        column: x => x.DataEntryId,
                        principalTable: "DataEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_CaseId",
                table: "ActionDefinitions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionDefinitions_Name",
                table: "ActionDefinitions",
                column: "Name",
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
                name: "IX_DataDefinitions_CaseId",
                table: "DataDefinitions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DataDefinitions_Name",
                table: "DataDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_CaseId",
                table: "DataEntries",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_DataDefinitionId_DataSetId_CaseId",
                table: "DataEntries",
                columns: new[] { "DataDefinitionId", "DataSetId", "CaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataEntries_DataSetId",
                table: "DataEntries",
                column: "DataSetId");

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
                name: "IX_Tags_CaseId",
                table: "Tags",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DataEntryId",
                table: "Tags",
                column: "DataEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionDefinitions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "DataEntries");

            migrationBuilder.DropTable(
                name: "DataDefinitions");

            migrationBuilder.DropTable(
                name: "DataSets");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
