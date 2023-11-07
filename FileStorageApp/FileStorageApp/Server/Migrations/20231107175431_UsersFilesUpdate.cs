using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UsersFilesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetadataUser_FilesMetadata_FilesId",
                table: "FileMetadataUser");

            migrationBuilder.DropForeignKey(
                name: "FK_FileMetadataUser_Users_UsersId",
                table: "FileMetadataUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileMetadataUser",
                table: "FileMetadataUser");

            migrationBuilder.RenameTable(
                name: "FileMetadataUser",
                newName: "UserFiles");

            migrationBuilder.RenameIndex(
                name: "IX_FileMetadataUser_UsersId",
                table: "UserFiles",
                newName: "IX_UserFiles_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFiles",
                table: "UserFiles",
                columns: new[] { "FilesId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFiles_FilesMetadata_FilesId",
                table: "UserFiles",
                column: "FilesId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFiles_Users_UsersId",
                table: "UserFiles",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFiles_FilesMetadata_FilesId",
                table: "UserFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFiles_Users_UsersId",
                table: "UserFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFiles",
                table: "UserFiles");

            migrationBuilder.RenameTable(
                name: "UserFiles",
                newName: "FileMetadataUser");

            migrationBuilder.RenameIndex(
                name: "IX_UserFiles_UsersId",
                table: "FileMetadataUser",
                newName: "IX_FileMetadataUser_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileMetadataUser",
                table: "FileMetadataUser",
                columns: new[] { "FilesId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetadataUser_FilesMetadata_FilesId",
                table: "FileMetadataUser",
                column: "FilesId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetadataUser_Users_UsersId",
                table: "FileMetadataUser",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
