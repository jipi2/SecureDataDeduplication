using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class ModUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilesMetadata_Users_UserId",
                table: "FilesMetadata");

            migrationBuilder.DropIndex(
                name: "IX_FilesMetadata_UserId",
                table: "FilesMetadata");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FilesMetadata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FilesMetadata",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilesMetadata_UserId",
                table: "FilesMetadata",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilesMetadata_Users_UserId",
                table: "FilesMetadata",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
