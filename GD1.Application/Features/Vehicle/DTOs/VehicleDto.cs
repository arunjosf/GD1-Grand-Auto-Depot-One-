using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class VehicleDto
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public double LengthFeet { get; set; }
        public double WidthFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? DocumentUrls { get; set; }
        public int HealthScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VehicleImageDto> Images { get; set; } = [];
    }

    public class VehicleImageDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public long? EventId { get; set; }
    }
}
