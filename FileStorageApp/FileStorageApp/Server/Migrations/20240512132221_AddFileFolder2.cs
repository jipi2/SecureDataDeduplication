using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFileFolder2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileFolders_UserFiles_UserFileId",
                table: "FileFolders");

            migrationBuilder.AlterColumn<int>(
                name: "UserFileId",
                table: "FileFolders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FileFolders_UserFiles_UserFileId",
                table: "FileFolders",
                column: "UserFileId",
                principalTable: "UserFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileFolders_UserFiles_UserFileId",
                table: "FileFolders");

            migrationBuilder.AlterColumn<int>(
                name: "UserFileId",
                table: "FileFolders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FileFolders_UserFiles_UserFileId",
                table: "FileFolders",
                column: "UserFileId",
                principalTable: "UserFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
