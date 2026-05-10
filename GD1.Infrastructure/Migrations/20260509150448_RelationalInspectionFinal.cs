using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelationalInspectionFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop existing tables that are part of the old prototype
            migrationBuilder.Sql("DROP TABLE IF EXISTS InspectionReports");
            
            // 2. Create the new InspectionAssignments table
            migrationBuilder.CreateTable(
                name: "InspectionAssignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    AgentId = table.Column<long>(type: "bigint", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionAssignments_FranchiseApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "FranchiseApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionAssignments_GD1Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "GD1Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3. Create the new InspectionReports table (clean from scratch)
            migrationBuilder.CreateTable(
                name: "InspectionReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OverallDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminDecision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionReports_InspectionAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "InspectionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 4. Create the InspectionItems table
            migrationBuilder.CreateTable(
                name: "InspectionItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<long>(type: "bigint", nullable: false),
                    LotUnitId = table.Column<long>(type: "bigint", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionItems_InspectionReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "InspectionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionItems_LotUnits_LotUnitId",
                        column: x => x.LotUnitId,
                        principalTable: "LotUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 5. Create the AgentRequests table
            migrationBuilder.CreateTable(
                name: "AgentRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentRequests_InspectionAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "InspectionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 6. Create Indexes
            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_AssignmentId",
                table: "InspectionReports",
                column: "AssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentRequests_AssignmentId",
                table: "AgentRequests",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAssignments_AgentId",
                table: "InspectionAssignments",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAssignments_ApplicationId",
                table: "InspectionAssignments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_LotUnitId",
                table: "InspectionItems",
                column: "LotUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_ReportId",
                table: "InspectionItems",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AgentRequests");
            migrationBuilder.DropTable(name: "InspectionItems");
            migrationBuilder.DropTable(name: "InspectionReports");
            migrationBuilder.DropTable(name: "InspectionAssignments");
        }
    }
}
