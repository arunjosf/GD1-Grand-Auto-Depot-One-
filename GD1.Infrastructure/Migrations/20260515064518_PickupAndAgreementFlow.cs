using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PickupAndAgreementFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssignedManagerId",
                table: "Bookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAgreementSigned",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPickupRequested",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PickupOtp",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingAgreements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeclined = table.Column<bool>(type: "bit", nullable: false),
                    VehicleSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LotSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingAgreements_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PickupVerifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    ManagerId = table.Column<long>(type: "bigint", nullable: false),
                    FrontImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RearImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeftSideImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RightSideImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InteriorImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineBayImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProofUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationDocUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickupVerifications_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAgreements_BookingId",
                table: "BookingAgreements",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupVerifications_BookingId",
                table: "PickupVerifications",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingAgreements");

            migrationBuilder.DropTable(
                name: "PickupVerifications");

            migrationBuilder.DropColumn(
                name: "AssignedManagerId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsAgreementSigned",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsPickupRequested",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PickupOtp",
                table: "Bookings");
        }
    }
}
