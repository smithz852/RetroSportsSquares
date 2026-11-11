using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSS_DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRecapEmailPreferenceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecapEmailsOptedOut",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UnsubscribeToken",
                table: "AspNetUsers",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecapEmailsOptedOut",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "AspNetUsers");
        }
    }
}
