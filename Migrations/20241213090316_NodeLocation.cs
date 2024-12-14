using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    /// <inheritdoc />
    public partial class NodeLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Consumers_ConsumerId",
                table: "Warehouses");

            migrationBuilder.AlterColumn<int>(
                name: "ConsumerId",
                table: "Warehouses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isAirport",
                table: "Nodes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "x",
                table: "Nodes",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "y",
                table: "Nodes",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Consumers_ConsumerId",
                table: "Warehouses",
                column: "ConsumerId",
                principalTable: "Consumers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Consumers_ConsumerId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "isAirport",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "x",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "y",
                table: "Nodes");

            migrationBuilder.AlterColumn<int>(
                name: "ConsumerId",
                table: "Warehouses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Consumers_ConsumerId",
                table: "Warehouses",
                column: "ConsumerId",
                principalTable: "Consumers",
                principalColumn: "Id");
        }
    }
}
