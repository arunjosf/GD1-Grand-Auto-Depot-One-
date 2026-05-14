using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRootExtraFacilitiesFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents");

            migrationBuilder.DropColumn(
                name: "ExtraFacilities",
                table: "FranchiseApplications");

            migrationBuilder.Sql("UPDATE FranchiseApplications SET Latitude = 0 WHERE Latitude IS NULL");
            migrationBuilder.Sql("UPDATE FranchiseApplications SET Longitude = 0 WHERE Longitude IS NULL");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "FranchiseApplications",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "FranchiseApplications",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "FranchiseApplications",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "FranchiseApplications",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "ExtraFacilities",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents",
                column: "UserId");
        }
    }
}
