
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramDesigner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexToNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Nodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TemplateId",
                table: "Nodes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrerequisiteTemplateId",
                table: "NodePrerequisites",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MissingPrerequisites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProgramId = table.Column<string>(type: "text", nullable: false),
                    NodeId = table.Column<string>(type: "text", nullable: false),
                    MissingPrerequisiteTemplateId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingPrerequisites", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_TemplateId",
                table: "Nodes",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NodePrerequisites_PrerequisiteTemplateId",
                table: "NodePrerequisites",
                column: "PrerequisiteTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissingPrerequisites");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_TemplateId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_NodePrerequisites_PrerequisiteTemplateId",
                table: "NodePrerequisites");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "PrerequisiteTemplateId",
                table: "NodePrerequisites");
        }
    }
}
