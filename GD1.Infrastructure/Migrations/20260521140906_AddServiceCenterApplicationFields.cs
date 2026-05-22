using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OemCertificateUrl",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedBrands",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OemCertificateUrl",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedBrands",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OemCertificateUrl",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "SupportedBrands",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "OemCertificateUrl",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "SupportedBrands",
                table: "FranchiseApplications");
        }
    }
}
