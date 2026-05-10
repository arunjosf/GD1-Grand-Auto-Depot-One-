using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorageLotImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraImageUrls",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "LeftSideImageUrl",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "RearImageUrl",
                table: "StorageLots");

            migrationBuilder.RenameColumn(
                name: "RightSideImageUrl",
                table: "StorageLots",
                newName: "OtherImageUrls");

            migrationBuilder.AlterColumn<string>(
                name: "FrontImageUrl",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OtherImageUrls",
                table: "StorageLots",
                newName: "RightSideImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "FrontImageUrl",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ExtraImageUrls",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeftSideImageUrl",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RearImageUrl",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
