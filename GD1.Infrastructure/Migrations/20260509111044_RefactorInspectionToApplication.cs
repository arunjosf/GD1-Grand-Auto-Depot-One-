using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInspectionToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionReports_LotUnits_LotUnitId",
                table: "InspectionReports");

            migrationBuilder.DropIndex(
                name: "IX_InspectionReports_LotUnitId",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "LotUnitId",
                table: "InspectionReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LotUnitId",
                table: "InspectionReports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_LotUnitId",
                table: "InspectionReports",
                column: "LotUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionReports_LotUnits_LotUnitId",
                table: "InspectionReports",
                column: "LotUnitId",
                principalTable: "LotUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
