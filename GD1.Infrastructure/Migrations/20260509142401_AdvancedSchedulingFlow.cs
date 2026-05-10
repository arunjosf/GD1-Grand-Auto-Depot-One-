using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedSchedulingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppealStatus",
                table: "InspectionReports",
                newName: "AgentAppealStatus");

            migrationBuilder.RenameColumn(
                name: "AppealDescription",
                table: "InspectionReports",
                newName: "AgentAppealDescription");

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduleRequestDate",
                table: "InspectionReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredInspectionDate",
                table: "FranchiseApplications",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RescheduleRequestDate",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "PreferredInspectionDate",
                table: "FranchiseApplications");

            migrationBuilder.RenameColumn(
                name: "AgentAppealStatus",
                table: "InspectionReports",
                newName: "AppealStatus");

            migrationBuilder.RenameColumn(
                name: "AgentAppealDescription",
                table: "InspectionReports",
                newName: "AppealDescription");
        }
    }
}
