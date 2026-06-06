
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(@"Server=localhost;Database=GD1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

using var db = new AppDbContext(optionsBuilder.Options);
var slot = db.VehicleStorageSlots.FirstOrDefault(x => x.SlotNumber == "A-103");
if (slot != null) {
    Console.WriteLine($"Slot A-103 ImageUrl: {slot.ImageUrl ?? "NULL"}");
    Console.WriteLine($"Slot A-103 length of ImageUrl: {slot.ImageUrl?.Length}");
} else {
    Console.WriteLine("Slot A-103 not found.");
}

