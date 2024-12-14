using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    /// <inheritdoc />
    public partial class CountryAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Access",
                table: "Countries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Access",
                table: "Countries");
        }
    }
}
