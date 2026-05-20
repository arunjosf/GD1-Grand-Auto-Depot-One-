using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergedAgentUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop constraints FIRST to allow updating values freely
            migrationBuilder.DropForeignKey(name: "FK_InspectionAssignments_GD1Agents_AgentId", table: "InspectionAssignments");
            migrationBuilder.DropForeignKey(name: "FK_GD1Agents_Users_UserId", table: "GD1Agents");
            migrationBuilder.DropIndex(name: "IX_GD1Agents_UserId", table: "GD1Agents");

            // 2. Update dependent table (InspectionAssignments) to use UserIds instead of old AgentIds
            migrationBuilder.Sql("UPDATE ia SET ia.AgentId = a.UserId FROM InspectionAssignments ia JOIN GD1Agents a ON ia.AgentId = a.Id");
            
            // 3. Drop old PK (Identity)
            migrationBuilder.Sql("ALTER TABLE GD1Agents DROP CONSTRAINT PK_GD1Agents");

            // 4. Drop the old Id column and rename UserId to Id
            migrationBuilder.DropColumn(name: "Id", table: "GD1Agents");
            migrationBuilder.RenameColumn(name: "UserId", table: "GD1Agents", newName: "Id");

            // 5. Re-establish PK on the new Id (which is the former UserId)
            migrationBuilder.AddPrimaryKey(name: "PK_GD1Agents", table: "GD1Agents", column: "Id");

            // 6. Add Foreign Keys back
            migrationBuilder.AddForeignKey(
                name: "FK_GD1Agents_Users_Id",
                table: "GD1Agents",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 7. Update the InspectionAssignments FK to point to the new PK
            migrationBuilder.AddForeignKey(
                name: "FK_InspectionAssignments_GD1Agents_AgentId",
                table: "InspectionAssignments",
                column: "AgentId",
                principalTable: "GD1Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GD1Agents_Users_Id",
                table: "GD1Agents");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GD1Agents",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "GD1Agents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_GD1Agents_UserId",
                table: "GD1Agents",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GD1Agents_Users_UserId",
                table: "GD1Agents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
