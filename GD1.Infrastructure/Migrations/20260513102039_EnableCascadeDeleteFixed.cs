using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeleteFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InspectionAssignments_FranchiseApplications_ApplicationId' AND parent_object_id = OBJECT_ID('InspectionAssignments'))
                BEGIN
                    ALTER TABLE [InspectionAssignments] DROP CONSTRAINT [FK_InspectionAssignments_FranchiseApplications_ApplicationId]
                END");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InspectionItems_LotUnits_LotUnitId' AND parent_object_id = OBJECT_ID('InspectionItems'))
                BEGIN
                    ALTER TABLE [InspectionItems] DROP CONSTRAINT [FK_InspectionItems_LotUnits_LotUnitId]
                END");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionAssignments_FranchiseApplications_ApplicationId",
                table: "InspectionAssignments",
                column: "ApplicationId",
                principalTable: "FranchiseApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionItems_LotUnits_LotUnitId",
                table: "InspectionItems",
                column: "LotUnitId",
                principalTable: "LotUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionAssignments_FranchiseApplications_ApplicationId",
                table: "InspectionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionItems_LotUnits_LotUnitId",
                table: "InspectionItems");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionAssignments_FranchiseApplications_ApplicationId",
                table: "InspectionAssignments",
                column: "ApplicationId",
                principalTable: "FranchiseApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionItems_LotUnits_LotUnitId",
                table: "InspectionItems",
                column: "LotUnitId",
                principalTable: "LotUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
