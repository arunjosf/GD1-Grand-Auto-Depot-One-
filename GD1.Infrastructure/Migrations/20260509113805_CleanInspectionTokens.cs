using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanInspectionTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InspectionReports_AccessToken",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "PasscodeHash",
                table: "InspectionReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "InspectionReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "InspectionReports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PasscodeHash",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_AccessToken",
                table: "InspectionReports",
                column: "AccessToken",
                unique: true);
        }
    }
}
