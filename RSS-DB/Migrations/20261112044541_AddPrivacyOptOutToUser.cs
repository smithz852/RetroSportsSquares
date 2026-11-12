using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSS_DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyOptOutToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DoNotSellOrShare",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DoNotSellOrShareSetAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoNotSellOrShare",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DoNotSellOrShareSetAt",
                table: "AspNetUsers");
        }
    }
}
