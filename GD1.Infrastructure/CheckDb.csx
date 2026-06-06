
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(@"Server=localhost;Database=GD1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

using var db = new AppDbContext(optionsBuilder.Options);
var p = db.VehicleStorageProperties.Include(x => x.ActivePropertyImages).Include(x => x.Slots).FirstOrDefault(x => x.Name.Contains("Neospace"));
if (p != null) {
    Console.WriteLine($"Property: {p.Name}");
    Console.WriteLine($"ActivePropertyImages Count: {p.ActivePropertyImages.Count}");
    foreach (var img in p.ActivePropertyImages) Console.WriteLine($"- Image: {img.ImageUrl}");
    Console.WriteLine($"Slots Count: {p.Slots.Count}");
    foreach (var slot in p.Slots) Console.WriteLine($"- Slot: {slot.SlotNumber}, Image: {slot.ImageUrl}");
} else {
    Console.WriteLine("Property not found.");
}

