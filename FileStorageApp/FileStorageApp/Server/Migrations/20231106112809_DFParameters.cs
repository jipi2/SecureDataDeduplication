using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class DFParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "G",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "P",
                table: "Users");
        }
    }
}
