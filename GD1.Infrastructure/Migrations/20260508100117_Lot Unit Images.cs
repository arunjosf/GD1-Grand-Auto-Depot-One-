using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LotUnitImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_LotUnits_LotUnitId",
                table: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_LotUnitId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "FrontImageUrl",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "OtherImageUrls",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "LotUnitId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "FrontImageUrl",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "OtherImageUrls",
                table: "FranchiseApplications");

            migrationBuilder.AddColumn<long>(
                name: "LotUnitId",
                table: "StorageLots",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "PropertyImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LotUnitImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotUnitId = table.Column<long>(type: "bigint", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotUnitImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotUnitImages_LotUnits_LotUnitId",
                        column: x => x.LotUnitId,
                        principalTable: "LotUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotUnitImages_LotUnitId",
                table: "LotUnitImages",
                column: "LotUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotUnitImages");

            migrationBuilder.DropColumn(
                name: "LotUnitId",
                table: "StorageLots");

            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "PropertyImages");

            migrationBuilder.AddColumn<string>(
                name: "FrontImageUrl",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OtherImageUrls",
                table: "StorageLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LotUnitId",
                table: "PropertyImages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontImageUrl",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OtherImageUrls",
                table: "FranchiseApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_LotUnitId",
                table: "PropertyImages",
                column: "LotUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_LotUnits_LotUnitId",
                table: "PropertyImages",
                column: "LotUnitId",
                principalTable: "LotUnits",
                principalColumn: "Id");
        }
    }
}
