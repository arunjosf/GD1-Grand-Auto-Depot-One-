using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ApplicationFee",
                table: "ServiceCenterPartneringApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FeeStatus",
                table: "ServiceCenterPartneringApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeeTransactionId",
                table: "ServiceCenterPartneringApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAiVerified",
                table: "ServiceCenterPartneringApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredInspectionDate",
                table: "ServiceCenterPartneringApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "ServiceCenterPartneringApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationFee",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "FeeStatus",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "FeeTransactionId",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "IsAiVerified",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "PreferredInspectionDate",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "ServiceCenterPartneringApplications");
        }
    }
}
