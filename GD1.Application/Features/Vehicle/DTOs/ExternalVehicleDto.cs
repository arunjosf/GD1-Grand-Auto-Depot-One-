using System;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class ExternalVehicleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Category { get; set; } = "Standard";
        public string? ImageUrl { get; set; }
    }
}
