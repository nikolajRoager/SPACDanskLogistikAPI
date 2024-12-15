using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    /// <inheritdoc />
    public partial class NodeNeighbours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NodeMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConnectionId = table.Column<int>(type: "int", nullable: false),
                    EndId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeMapping_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeMapping_Nodes_EndId",
                        column: x => x.EndId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NodeMapping_ConnectionId",
                table: "NodeMapping",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeMapping_EndId",
                table: "NodeMapping",
                column: "EndId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeMapping");
        }
    }
}
