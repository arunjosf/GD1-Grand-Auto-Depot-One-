using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDimensionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HeightFeet",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LengthFeet",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WidthFeet",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HeightFeet",
                table: "LotUnits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LengthFeet",
                table: "LotUnits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WidthFeet",
                table: "LotUnits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightFeet",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LengthFeet",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "WidthFeet",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HeightFeet",
                table: "LotUnits");

            migrationBuilder.DropColumn(
                name: "LengthFeet",
                table: "LotUnits");

            migrationBuilder.DropColumn(
                name: "WidthFeet",
                table: "LotUnits");
        }
    }
}
