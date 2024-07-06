using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "G",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "P",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ServerDHPrivate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ServerDHPublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SymKey",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "G",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "P",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServerDHPrivate",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServerDHPublic",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SymKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
