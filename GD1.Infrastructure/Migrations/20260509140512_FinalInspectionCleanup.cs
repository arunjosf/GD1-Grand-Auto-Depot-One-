using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalInspectionCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppealAt",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealReason",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AppealResolvedAt",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppealAt",
                table: "InspectionReports",
                type: "datetime2",
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
    }
}
