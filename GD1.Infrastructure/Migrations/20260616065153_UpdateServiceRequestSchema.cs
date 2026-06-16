using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequestSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletionPhotos",
                table: "ServiceRequests",
                newName: "Instructions");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Mechanics");

            migrationBuilder.RenameColumn(
                name: "Instructions",
                table: "ServiceRequests",
                newName: "CompletionPhotos");
        }
    }
}
