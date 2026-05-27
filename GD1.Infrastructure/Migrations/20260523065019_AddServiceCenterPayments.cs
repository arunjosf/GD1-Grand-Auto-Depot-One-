using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextPaymentDate",
                table: "ServiceCenters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ServiceCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPaymentDate",
                table: "ServiceCenterPartneringApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ServiceCenterPartneringApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextPaymentDate",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "NextPaymentDate",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ServiceCenterPartneringApplications");
        }
    }
}
