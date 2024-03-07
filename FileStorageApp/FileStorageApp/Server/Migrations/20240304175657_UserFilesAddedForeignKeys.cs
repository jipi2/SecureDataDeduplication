using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UserFilesAddedForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFiles_FilesMetadata_FileMetadataId",
                table: "UserFiles");

            migrationBuilder.DropIndex(
                name: "IX_UserFiles_FileMetadataId",
                table: "UserFiles");

            migrationBuilder.DropColumn(
                name: "FileMetadataId",
                table: "UserFiles");

            migrationBuilder.AlterColumn<int>(
                name: "FileId",
                table: "UserFiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_UserFiles_FileId",
                table: "UserFiles",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFiles_FilesMetadata_FileId",
                table: "UserFiles",
                column: "FileId",
                principalTable: "FilesMetadata",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFiles_FilesMetadata_FileId",
                table: "UserFiles");

            migrationBuilder.DropIndex(
                name: "IX_UserFiles_FileId",
                table: "UserFiles");

            migrationBuilder.AlterColumn<int>(
                name: "FileId",
                table: "UserFiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileMetadataId",
                table: "UserFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserFiles_FileMetadataId",
                table: "UserFiles",
                column: "FileMetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFiles_FilesMetadata_FileMetadataId",
                table: "UserFiles",
                column: "FileMetadataId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
