using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPkcs12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Base64RSAEncPrivateKey",
                table: "Users");

            migrationBuilder.AddColumn<byte[]>(
                name: "Pkcs12File",
                table: "Users",
                type: "VARBINARY",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pkcs12File",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Base64RSAEncPrivateKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
