using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsAiVerified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAiVerified",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "IsAiVerified",
                table: "ServiceCenterPartneringApplications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiVerified",
                table: "ServiceCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAiVerified",
                table: "ServiceCenterPartneringApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
