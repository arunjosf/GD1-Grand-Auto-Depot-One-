using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreBookingAgreementFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeclined",
                table: "BookingAgreements");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SignedAt",
                table: "BookingAgreements",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<long>(
                name: "BookingId",
                table: "BookingAgreements",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "BookingAgreements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PropertyId",
                table: "BookingAgreements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BookingAgreements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "VehicleId",
                table: "BookingAgreements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_BookingAgreements_OwnerId",
                table: "BookingAgreements",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAgreements_PropertyId",
                table: "BookingAgreements",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAgreements_VehicleId",
                table: "BookingAgreements",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAgreements_Users_OwnerId",
                table: "BookingAgreements",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAgreements_VehicleStorageProperties_PropertyId",
                table: "BookingAgreements",
                column: "PropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAgreements_Vehicles_VehicleId",
                table: "BookingAgreements",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAgreements_Users_OwnerId",
                table: "BookingAgreements");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingAgreements_VehicleStorageProperties_PropertyId",
                table: "BookingAgreements");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingAgreements_Vehicles_VehicleId",
                table: "BookingAgreements");

            migrationBuilder.DropIndex(
                name: "IX_BookingAgreements_OwnerId",
                table: "BookingAgreements");

            migrationBuilder.DropIndex(
                name: "IX_BookingAgreements_PropertyId",
                table: "BookingAgreements");

            migrationBuilder.DropIndex(
                name: "IX_BookingAgreements_VehicleId",
                table: "BookingAgreements");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "BookingAgreements");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "BookingAgreements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BookingAgreements");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "BookingAgreements");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SignedAt",
                table: "BookingAgreements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BookingId",
                table: "BookingAgreements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                table: "BookingAgreements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
