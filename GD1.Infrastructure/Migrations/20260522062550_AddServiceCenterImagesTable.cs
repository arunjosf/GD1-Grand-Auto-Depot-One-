using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCenterImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacilityImages",
                table: "ServiceCenterPartneringApplications");

            migrationBuilder.CreateTable(
                name: "ServiceCenterImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: true),
                    ServiceCenterId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCenterImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCenterImages_ServiceCenterPartneringApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ServiceCenterPartneringApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceCenterImages_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalTable: "ServiceCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenterImages_ApplicationId",
                table: "ServiceCenterImages",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenterImages_ServiceCenterId",
                table: "ServiceCenterImages",
                column: "ServiceCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceCenterImages");

            migrationBuilder.AddColumn<string>(
                name: "FacilityImages",
                table: "ServiceCenterPartneringApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
