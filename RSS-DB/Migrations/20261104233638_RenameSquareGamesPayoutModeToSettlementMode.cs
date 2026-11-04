using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSS_DB.Migrations
{
    /// <inheritdoc />
    public partial class RenameSquareGamesPayoutModeToSettlementMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayoutMode",
                table: "SquareGames",
                newName: "SettlementMode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SettlementMode",
                table: "SquareGames",
                newName: "PayoutMode");
        }
    }
}
