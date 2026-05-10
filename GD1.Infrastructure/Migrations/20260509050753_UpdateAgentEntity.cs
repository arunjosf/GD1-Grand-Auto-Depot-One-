using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "GD1Agents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "GD1Agents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "GD1Agents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GD1Agents_Users_UserId",
                table: "GD1Agents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GD1Agents_Users_UserId",
                table: "GD1Agents");

            migrationBuilder.DropIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "GD1Agents");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "GD1Agents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GD1Agents");
        }
    }
}
