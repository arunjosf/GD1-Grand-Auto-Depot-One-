using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalFranchiseSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppealAt",
                table: "InspectionReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppealDescription",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppealReason",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppealResolvedAt",
                table: "InspectionReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppealStatus",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverallDescription",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleAdminRemarks",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleReason",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduleRequestedDate",
                table: "InspectionReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleStatus",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppealAt",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealDescription",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealReason",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealResolvedAt",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealStatus",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "OverallDescription",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "RescheduleAdminRemarks",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "RescheduleReason",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "RescheduleRequestedDate",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "RescheduleStatus",
                table: "InspectionReports");
        }
    }
}
