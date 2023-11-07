using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaAddedTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IvFile",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "key",
                table: "FilesMetadata",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "iv",
                table: "FilesMetadata",
                newName: "Iv");

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "FilesMetadata",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tag",
                table: "FilesMetadata");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "FilesMetadata",
                newName: "key");

            migrationBuilder.RenameColumn(
                name: "Iv",
                table: "FilesMetadata",
                newName: "iv");

            migrationBuilder.AddColumn<string>(
                name: "IvFile",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
