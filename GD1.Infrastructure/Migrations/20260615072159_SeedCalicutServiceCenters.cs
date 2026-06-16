using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GD1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCalicutServiceCenters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<GD1.Domain.Entities.User>();
            
            var users = new[]
            {
                new { Id = 101L, Email = "jerry@yopmail.com", Name = "Jerry", Pass = "Jerry@1234" },
                new { Id = 102L, Email = "tom@yopmail.com", Name = "Tom", Pass = "Tom@1234" },
                new { Id = 103L, Email = "alice@yopmail.com", Name = "Alice", Pass = "Alice@1234" },
                new { Id = 104L, Email = "bob@yopmail.com", Name = "Bob", Pass = "Bob@1234" },
                new { Id = 105L, Email = "charlie@yopmail.com", Name = "Charlie", Pass = "Charlie@1234" }
            };

            foreach (var u in users)
            {
                migrationBuilder.InsertData(
                    table: "Users",
                    columns: new[] { "Id", "FullName", "Email", "PasswordHash", "Role", "IsEmailVerified", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt" },
                    values: new object[] { u.Id, u.Name, u.Email, hasher.HashPassword(null, u.Pass), 4, true, true, false, DateTime.UtcNow, DateTime.UtcNow }
                );
            }

            var centers = new[]
            {
                new { Id = 101L, AdminId = 101L, Name = "Maruthi Care Work Shop", Addr = "Olipram Kadavu Road", Lat = 11.1340, Lon = 75.8952 },
                new { Id = 102L, AdminId = 102L, Name = "Tom Auto Service", Addr = "Thenhipalam Junction", Lat = 11.1350, Lon = 75.8920 },
                new { Id = 103L, AdminId = 103L, Name = "Alice Car Care", Addr = "University Road", Lat = 11.1400, Lon = 75.9000 },
                new { Id = 104L, AdminId = 104L, Name = "Bob Motors", Addr = "Chelari", Lat = 11.1250, Lon = 75.8850 },
                new { Id = 105L, AdminId = 105L, Name = "Charlie Quick Fix", Addr = "Kakkanchery", Lat = 11.1500, Lon = 75.8900 }
            };

            foreach (var c in centers)
            {
                migrationBuilder.InsertData(
                    table: "ServiceCenters",
                    columns: new[] { "Id", "AdminId", "Name", "OwnerName", "PhoneNumber", "AddressLine", "City", "State", "Country", "PostalCode", "District", "Latitude", "Longitude", "Status", "IsVerified", "IsActive", "CoverageRadiusKm", "AverageRating", "IsDeleted", "CreatedAt", "UpdatedAt" },
                    values: new object[] { c.Id, c.AdminId, c.Name, users.First(u => u.Id == c.AdminId).Name, "9999999999", c.Addr, "Thenhipalam", "Kerala", "India", "673636", "Malappuram", c.Lat, c.Lon, "Approved", true, true, 25, 4.5m, false, DateTime.UtcNow, DateTime.UtcNow }
                );
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ServiceCenters WHERE Id IN (101, 102, 103, 104, 105)");
            migrationBuilder.Sql("DELETE FROM Users WHERE Id IN (101, 102, 103, 104, 105)");
        }
    }
}
