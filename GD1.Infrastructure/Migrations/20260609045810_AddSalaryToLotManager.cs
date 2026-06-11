using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryToLotManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "LotManagers",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "LotManagers");
        }
    }
}
