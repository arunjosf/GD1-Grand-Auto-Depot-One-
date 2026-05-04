using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.DTOs
{
    public class AddVehicleRequest
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string? DocumentUrls { get; set; }
        public List<VehicleImageRequest> Images { get; set; } = [];
    }

    public class VehicleImageRequest
    {
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }
}
