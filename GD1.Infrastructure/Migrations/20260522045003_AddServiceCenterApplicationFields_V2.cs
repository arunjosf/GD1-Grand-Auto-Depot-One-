using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterApplicationFields_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAiVerified",
                table: "ServiceCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerIdProofUrl",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "District",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "IsAiVerified",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "OwnerIdProofUrl",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "ServiceCenters");
        }
    }
}
