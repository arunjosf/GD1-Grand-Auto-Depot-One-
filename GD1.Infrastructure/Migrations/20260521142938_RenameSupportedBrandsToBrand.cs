using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSupportedBrandsToBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportedBrands",
                table: "ServiceCenters",
                newName: "SupportedBrand");

            migrationBuilder.RenameColumn(
                name: "SupportedBrands",
                table: "FranchiseApplications",
                newName: "SupportedBrand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportedBrand",
                table: "ServiceCenters",
                newName: "SupportedBrands");

            migrationBuilder.RenameColumn(
                name: "SupportedBrand",
                table: "FranchiseApplications",
                newName: "SupportedBrands");
        }
    }
}
