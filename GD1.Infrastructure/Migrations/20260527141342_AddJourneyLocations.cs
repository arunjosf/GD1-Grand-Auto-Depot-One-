using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJourneyLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredVehicles_Bookings_BookingId",
                table: "StoredVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredVehicles_Vehicles_VehicleId",
                table: "StoredVehicles");

            migrationBuilder.CreateTable(
                name: "JourneyLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyLocations_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLocations_BookingId",
                table: "JourneyLocations",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredVehicles_Bookings_BookingId",
                table: "StoredVehicles",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredVehicles_Vehicles_VehicleId",
                table: "StoredVehicles",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredVehicles_Bookings_BookingId",
                table: "StoredVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredVehicles_Vehicles_VehicleId",
                table: "StoredVehicles");

            migrationBuilder.DropTable(
                name: "JourneyLocations");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredVehicles_Bookings_BookingId",
                table: "StoredVehicles",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredVehicles_Vehicles_VehicleId",
                table: "StoredVehicles",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
