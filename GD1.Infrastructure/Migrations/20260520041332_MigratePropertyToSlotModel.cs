using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigratePropertyToSlotModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_LotSlots_SlotId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_StorageLots_LotId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_StorageLots_LotId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_LotManagers_StorageLots_LotId",
                table: "LotManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_StorageLots_LotId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "InspectionItems");

            migrationBuilder.DropTable(
                name: "LotSlots");

            migrationBuilder.DropTable(
                name: "LotUnitImages");

            migrationBuilder.DropTable(
                name: "StorageLots");

            migrationBuilder.DropTable(
                name: "LotUnits");

            migrationBuilder.DropColumn(
                name: "DecisionAt",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "InspectionReports");

            migrationBuilder.RenameColumn(
                name: "LotId",
                table: "Reviews",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_LotId",
                table: "Reviews",
                newName: "IX_Reviews_PropertyId");

            migrationBuilder.RenameColumn(
                name: "LotId",
                table: "LotManagers",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_LotManagers_LotId",
                table: "LotManagers",
                newName: "IX_LotManagers_PropertyId");

            migrationBuilder.RenameColumn(
                name: "AgentRemarks",
                table: "InspectionReports",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "LotId",
                table: "Complaints",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Complaints_LotId",
                table: "Complaints",
                newName: "IX_Complaints_PropertyId");

            migrationBuilder.RenameColumn(
                name: "LotId",
                table: "Bookings",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_LotId_StartDate_EndDate",
                table: "Bookings",
                newName: "IX_Bookings_PropertyId_StartDate_EndDate");

            migrationBuilder.AddColumn<long>(
                name: "InspectionReportId",
                table: "PropertyImages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VehicleStoragePropertyId",
                table: "PropertyImages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminDecision",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCCTV",
                table: "FranchiseApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFireSafety",
                table: "FranchiseApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSecurity",
                table: "FranchiseApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWashingArea",
                table: "FranchiseApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWorkshop",
                table: "FranchiseApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FranchiseSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    SlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SquareFeet = table.Column<double>(type: "float", nullable: false),
                    HeightFeet = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseSlots_FranchiseApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "FranchiseApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSlotItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<long>(type: "bigint", nullable: false),
                    SlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SquareFeet = table.Column<double>(type: "float", nullable: false),
                    HeightFeet = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSlotItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSlotItems_InspectionReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "InspectionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleStorageProperties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotOwnerId = table.Column<long>(type: "bigint", nullable: false),
                    LotCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasCCTV = table.Column<bool>(type: "bit", nullable: false),
                    HasSecurity = table.Column<bool>(type: "bit", nullable: false),
                    HasFireSafety = table.Column<bool>(type: "bit", nullable: false),
                    HasWorkshopBay = table.Column<bool>(type: "bit", nullable: false),
                    HasWashingArea = table.Column<bool>(type: "bit", nullable: false),
                    ExtraFacilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PricePerDay = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleStorageProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleStorageProperties_Users_LotOwnerId",
                        column: x => x.LotOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleStorageSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    SlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlotType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    SquareFeet = table.Column<double>(type: "float", nullable: false),
                    HeightFeet = table.Column<double>(type: "float", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleStorageSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleStorageSlots_VehicleStorageProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "VehicleStorageProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_InspectionReportId",
                table: "PropertyImages",
                column: "InspectionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_VehicleStoragePropertyId",
                table: "PropertyImages",
                column: "VehicleStoragePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseSlots_ApplicationId",
                table: "FranchiseSlots",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSlotItems_ReportId",
                table: "InspectionSlotItems",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStorageProperties_LotCode",
                table: "VehicleStorageProperties",
                column: "LotCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStorageProperties_LotOwnerId",
                table: "VehicleStorageProperties",
                column: "LotOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStorageSlots_PropertyId",
                table: "VehicleStorageSlots",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_VehicleStorageProperties_PropertyId",
                table: "Bookings",
                column: "PropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_VehicleStorageSlots_SlotId",
                table: "Bookings",
                column: "SlotId",
                principalTable: "VehicleStorageSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_VehicleStorageProperties_PropertyId",
                table: "Complaints",
                column: "PropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotManagers_VehicleStorageProperties_PropertyId",
                table: "LotManagers",
                column: "PropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_InspectionReports_InspectionReportId",
                table: "PropertyImages",
                column: "InspectionReportId",
                principalTable: "InspectionReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_VehicleStorageProperties_VehicleStoragePropertyId",
                table: "PropertyImages",
                column: "VehicleStoragePropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_VehicleStorageProperties_PropertyId",
                table: "Reviews",
                column: "PropertyId",
                principalTable: "VehicleStorageProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_VehicleStorageProperties_PropertyId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_VehicleStorageSlots_SlotId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_VehicleStorageProperties_PropertyId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_LotManagers_VehicleStorageProperties_PropertyId",
                table: "LotManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_InspectionReports_InspectionReportId",
                table: "PropertyImages");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_VehicleStorageProperties_VehicleStoragePropertyId",
                table: "PropertyImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_VehicleStorageProperties_PropertyId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "FranchiseSlots");

            migrationBuilder.DropTable(
                name: "InspectionSlotItems");

            migrationBuilder.DropTable(
                name: "VehicleStorageSlots");

            migrationBuilder.DropTable(
                name: "VehicleStorageProperties");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_InspectionReportId",
                table: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_VehicleStoragePropertyId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "InspectionReportId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "VehicleStoragePropertyId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "HasCCTV",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "HasFireSafety",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "HasSecurity",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "HasWashingArea",
                table: "FranchiseApplications");

            migrationBuilder.DropColumn(
                name: "HasWorkshop",
                table: "FranchiseApplications");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Reviews",
                newName: "LotId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_PropertyId",
                table: "Reviews",
                newName: "IX_Reviews_LotId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "LotManagers",
                newName: "LotId");

            migrationBuilder.RenameIndex(
                name: "IX_LotManagers_PropertyId",
                table: "LotManagers",
                newName: "IX_LotManagers_LotId");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "InspectionReports",
                newName: "AgentRemarks");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Complaints",
                newName: "LotId");

            migrationBuilder.RenameIndex(
                name: "IX_Complaints_PropertyId",
                table: "Complaints",
                newName: "IX_Complaints_LotId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Bookings",
                newName: "LotId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_PropertyId_StartDate_EndDate",
                table: "Bookings",
                newName: "IX_Bookings_LotId_StartDate_EndDate");

            migrationBuilder.AlterColumn<string>(
                name: "AdminDecision",
                table: "InspectionReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionAt",
                table: "InspectionReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "InspectionReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LotUnits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FranchiseApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    AssignedLotCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraFacilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasCCTV = table.Column<bool>(type: "bit", nullable: false),
                    HasFireSafety = table.Column<bool>(type: "bit", nullable: false),
                    HasSecurity = table.Column<bool>(type: "bit", nullable: false),
                    HasWashingArea = table.Column<bool>(type: "bit", nullable: false),
                    HasWorkshop = table.Column<bool>(type: "bit", nullable: false),
                    HeightFeet = table.Column<double>(type: "float", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LengthFeet = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WidthFeet = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotUnits_FranchiseApplications_FranchiseApplicationId",
                        column: x => x.FranchiseApplicationId,
                        principalTable: "FranchiseApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StorageLots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotOwnerId = table.Column<long>(type: "bigint", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraFacilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasCCTV = table.Column<bool>(type: "bit", nullable: false),
                    HasFireSafety = table.Column<bool>(type: "bit", nullable: false),
                    HasSecurity = table.Column<bool>(type: "bit", nullable: false),
                    HasWashingArea = table.Column<bool>(type: "bit", nullable: false),
                    HasWorkshopBay = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    LotCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LotUnitId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    TotalSlots = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLots_Users_LotOwnerId",
                        column: x => x.LotOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotUnitId = table.Column<long>(type: "bigint", nullable: false),
                    ReportId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotUnitImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotUnitId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LotSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    QRCodeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SlotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlotType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotSlots_StorageLots_LotId",
                        column: x => x.LotId,
                        principalTable: "StorageLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_LotUnitId",
                table: "InspectionItems",
                column: "LotUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_ReportId",
                table: "InspectionItems",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_LotSlots_LotId",
                table: "LotSlots",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_LotUnitImages_LotUnitId",
                table: "LotUnitImages",
                column: "LotUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LotUnits_FranchiseApplicationId",
                table: "LotUnits",
                column: "FranchiseApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLots_LotCode",
                table: "StorageLots",
                column: "LotCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLots_LotOwnerId",
                table: "StorageLots",
                column: "LotOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_LotSlots_SlotId",
                table: "Bookings",
                column: "SlotId",
                principalTable: "LotSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_StorageLots_LotId",
                table: "Bookings",
                column: "LotId",
                principalTable: "StorageLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_StorageLots_LotId",
                table: "Complaints",
                column: "LotId",
                principalTable: "StorageLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotManagers_StorageLots_LotId",
                table: "LotManagers",
                column: "LotId",
                principalTable: "StorageLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_StorageLots_LotId",
                table: "Reviews",
                column: "LotId",
                principalTable: "StorageLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
