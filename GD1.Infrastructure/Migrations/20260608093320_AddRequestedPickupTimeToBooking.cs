using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedPickupTimeToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "BookingId",
                table: "ChatMessages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ServiceRequestId",
                table: "ChatMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedPickupTime",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ServiceRequestId",
                table: "ChatMessages",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ServiceRequests_ServiceRequestId",
                table: "ChatMessages",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ServiceRequests_ServiceRequestId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ServiceRequestId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "RequestedPickupTime",
                table: "Bookings");

            migrationBuilder.AlterColumn<long>(
                name: "BookingId",
                table: "ChatMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
