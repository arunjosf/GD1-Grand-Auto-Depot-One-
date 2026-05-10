using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorageLotFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraFacilities",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFireSafety",
                table: "StorageLots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExtraFacilities",
                table: "LotUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFireSafety",
                table: "LotUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExtraFacilities",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraFacilities",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "HasFireSafety",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "ExtraFacilities",
                table: "LotUnits");

            migrationBuilder.DropColumn(
                name: "HasFireSafety",
                table: "LotUnits");

            migrationBuilder.DropColumn(
                name: "ExtraFacilities",
                table: "FranchiseApplications");
        }
    }
}
