using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public long OwnerId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public bool IsHybrid { get; set; } = false;

        public string Category { get; set; } = string.Empty;
        public double LengthFeet { get; set; }
        public double WidthFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? DocumentUrls { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? VehicleRcUrl { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public int HealthScore { get; set; } = 100;

        public User Owner { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = [];
        public ICollection<VehicleImage> Images { get; set; } = [];
    }
}
