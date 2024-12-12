using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    /// <inheritdoc />
    public partial class SVGSnippet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "B",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "G",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "R",
                table: "Countries");

            migrationBuilder.AddColumn<int>(
                name: "OutlineId",
                table: "municipalities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeJureOutlineId",
                table: "Countries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Snippets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SvgContent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snippets", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_municipalities_OutlineId",
                table: "municipalities",
                column: "OutlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_DeJureOutlineId",
                table: "Countries",
                column: "DeJureOutlineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Snippets_DeJureOutlineId",
                table: "Countries",
                column: "DeJureOutlineId",
                principalTable: "Snippets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_municipalities_Snippets_OutlineId",
                table: "municipalities",
                column: "OutlineId",
                principalTable: "Snippets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Countries_Snippets_DeJureOutlineId",
                table: "Countries");

            migrationBuilder.DropForeignKey(
                name: "FK_municipalities_Snippets_OutlineId",
                table: "municipalities");

            migrationBuilder.DropTable(
                name: "Snippets");

            migrationBuilder.DropIndex(
                name: "IX_municipalities_OutlineId",
                table: "municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Countries_DeJureOutlineId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "OutlineId",
                table: "municipalities");

            migrationBuilder.DropColumn(
                name: "DeJureOutlineId",
                table: "Countries");

            migrationBuilder.AddColumn<sbyte>(
                name: "B",
                table: "Countries",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<sbyte>(
                name: "G",
                table: "Countries",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<sbyte>(
                name: "R",
                table: "Countries",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);
        }
    }
}
