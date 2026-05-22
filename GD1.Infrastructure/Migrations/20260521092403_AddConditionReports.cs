using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConditionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LotManagers_Users_ManagerId",
                table: "LotManagers");

            migrationBuilder.AddColumn<string>(
                name: "OdometerImageUrl",
                table: "PickupVerifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelfieUrl",
                table: "PickupVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PickupVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerSubmittedOtp",
                table: "PickupRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LotManagers_Users_ManagerId",
                table: "LotManagers",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LotManagers_Users_ManagerId",
                table: "LotManagers");

            migrationBuilder.DropColumn(
                name: "OdometerImageUrl",
                table: "PickupVerifications");

            migrationBuilder.DropColumn(
                name: "SelfieUrl",
                table: "PickupVerifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PickupVerifications");

            migrationBuilder.DropColumn(
                name: "OwnerSubmittedOtp",
                table: "PickupRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LotManagers_Users_ManagerId",
                table: "LotManagers",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
