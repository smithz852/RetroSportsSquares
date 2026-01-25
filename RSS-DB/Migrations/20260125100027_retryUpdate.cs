using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSS_DB.Migrations
{
    /// <inheritdoc />
    public partial class retryUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameType",
                table: "AvailableGames",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameType",
                table: "AvailableGames");
        }
    }
}
