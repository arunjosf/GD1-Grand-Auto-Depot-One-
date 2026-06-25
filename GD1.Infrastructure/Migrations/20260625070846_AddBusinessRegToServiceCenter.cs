using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessRegToServiceCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OemCertificateUrl",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "OemCertificateUrl",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.RenameColumn(
                name: "SupportedBrands",
                table: "ServiceCenters",
                newName: "BusinessRegistrationUrl");

            migrationBuilder.RenameColumn(
                name: "SupportedBrands",
                table: "ServiceCenterPartneringApplications",
                newName: "BusinessRegistrationUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BusinessRegistrationUrl",
                table: "ServiceCenters",
                newName: "SupportedBrands");

            migrationBuilder.RenameColumn(
                name: "BusinessRegistrationUrl",
                table: "ServiceCenterPartneringApplications",
                newName: "SupportedBrands");

            migrationBuilder.AddColumn<string>(
                name: "OemCertificateUrl",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OemCertificateUrl",
                table: "ServiceCenterPartneringApplications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
