using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReorderAgentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the FK from InspectionAssignments temporarily
            migrationBuilder.DropForeignKey(name: "FK_InspectionAssignments_GD1Agents_AgentId", table: "InspectionAssignments");

            // 2. Rename existing table
            migrationBuilder.Sql("EXEC sp_rename 'GD1Agents', 'GD1Agents_Old'");

            // 2.5 Drop the old constraints to avoid collision
            migrationBuilder.Sql("ALTER TABLE GD1Agents_Old DROP CONSTRAINT FK_GD1Agents_Users_Id");
            migrationBuilder.Sql("ALTER TABLE GD1Agents_Old DROP CONSTRAINT PK_GD1Agents");

            // 3. Create new table with Id as the FIRST column
            migrationBuilder.Sql(@"
                CREATE TABLE GD1Agents (
                    Id bigint NOT NULL,
                    City nvarchar(MAX) NOT NULL,
                    State nvarchar(MAX) NOT NULL,
                    CoverageArea nvarchar(MAX) NOT NULL,
                    Latitude float NULL,
                    Longitude float NULL,
                    IsActive bit NOT NULL,
                    CreatedAt datetime2 NOT NULL,
                    UpdatedAt datetime2 NOT NULL,
                    IsVerified bit NOT NULL,
                    InvitationToken nvarchar(MAX) NULL,
                    IdProofUrl nvarchar(MAX) NULL,
                    SelfieUrl nvarchar(MAX) NULL,
                    ApprovalStatus int NOT NULL,
                    PostalCode nvarchar(MAX) NULL,
                    IsDeleted bit NOT NULL,
                    CONSTRAINT PK_GD1Agents PRIMARY KEY (Id),
                    CONSTRAINT FK_GD1Agents_Users_Id FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
                )");

            // 4. Copy data from old table to new table
            migrationBuilder.Sql(@"
                INSERT INTO GD1Agents (Id, City, State, CoverageArea, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt, IsVerified, InvitationToken, IdProofUrl, SelfieUrl, ApprovalStatus, PostalCode, IsDeleted)
                SELECT Id, City, State, CoverageArea, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt, IsVerified, InvitationToken, IdProofUrl, SelfieUrl, ApprovalStatus, PostalCode, IsDeleted
                FROM GD1Agents_Old");

            // 5. Drop old table
            migrationBuilder.Sql("DROP TABLE GD1Agents_Old");

            // 6. Restore the FK from InspectionAssignments
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

        }
    }
}
