using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    /// <inheritdoc />
    public partial class NodeNeighboursMapFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeMappings_Connections_ConnectionId",
                table: "NodeMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeMappings_Nodes_EndId",
                table: "NodeMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeMappings_Nodes_StartId",
                table: "NodeMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeMappings",
                table: "NodeMappings");

            migrationBuilder.RenameTable(
                name: "NodeMappings",
                newName: "NodeMapping");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMappings_StartId",
                table: "NodeMapping",
                newName: "IX_NodeMapping_StartId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMappings_EndId",
                table: "NodeMapping",
                newName: "IX_NodeMapping_EndId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMappings_ConnectionId",
                table: "NodeMapping",
                newName: "IX_NodeMapping_ConnectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeMapping",
                table: "NodeMapping",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMapping_Connections_ConnectionId",
                table: "NodeMapping",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMapping_Nodes_EndId",
                table: "NodeMapping",
                column: "EndId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMapping_Nodes_StartId",
                table: "NodeMapping",
                column: "StartId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeMapping_Connections_ConnectionId",
                table: "NodeMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeMapping_Nodes_EndId",
                table: "NodeMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeMapping_Nodes_StartId",
                table: "NodeMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeMapping",
                table: "NodeMapping");

            migrationBuilder.RenameTable(
                name: "NodeMapping",
                newName: "NodeMappings");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMapping_StartId",
                table: "NodeMappings",
                newName: "IX_NodeMappings_StartId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMapping_EndId",
                table: "NodeMappings",
                newName: "IX_NodeMappings_EndId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeMapping_ConnectionId",
                table: "NodeMappings",
                newName: "IX_NodeMappings_ConnectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeMappings",
                table: "NodeMappings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMappings_Connections_ConnectionId",
                table: "NodeMappings",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMappings_Nodes_EndId",
                table: "NodeMappings",
                column: "EndId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeMappings_Nodes_StartId",
                table: "NodeMappings",
                column: "StartId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
