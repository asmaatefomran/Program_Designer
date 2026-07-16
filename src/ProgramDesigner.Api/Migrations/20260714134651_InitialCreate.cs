using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramDesigner.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentGroupId = table.Column<string>(type: "text", nullable: true),
                    NodeType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    GroupType = table.Column<int>(type: "integer", nullable: true),
                    RequiredSelections = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodePrerequisites",
                columns: table => new
                {
                    NodeId = table.Column<string>(type: "text", nullable: false),
                    PrerequisiteId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodePrerequisites", x => new { x.NodeId, x.PrerequisiteId });
                    table.ForeignKey(
                        name: "FK_NodePrerequisites_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodePrerequisites_Nodes_PrerequisiteId",
                        column: x => x.PrerequisiteId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RootGroupId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programs_Nodes_RootGroupId",
                        column: x => x.RootGroupId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodePrerequisites_PrerequisiteId",
                table: "NodePrerequisites",
                column: "PrerequisiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ParentGroupId",
                table: "Nodes",
                column: "ParentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_RootGroupId",
                table: "Programs",
                column: "RootGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodePrerequisites");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
