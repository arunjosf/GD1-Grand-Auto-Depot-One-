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
        public bool IsHybrid { get; set; }
        public string Category { get; set; } = string.Empty;
        public double LengthFeet { get; set; }
        public double WidthFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? VehicleRcUrl { get; set; }
        public int HealthScore { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsStored { get; set; }
        public string? LotName { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Location { get; set; }
        public DateTime? LastConditionUpdate { get; set; }
        public string? PickupStatus { get; set; }
        
        public List<VehicleImageDto> RecentOnDemandImages { get; set; } = [];
        public List<VehicleImageDto> RecentWeeklyCheckImages { get; set; } = [];
        public List<VehicleImageDto> PickupImages { get; set; } = [];
        public List<VehicleImageDto> LotArrivalImages { get; set; } = [];
        
        public List<VehicleJourneyEventDto> JourneyEvents { get; set; } = [];
    }

    public class VehicleJourneyEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<VehicleImageDto> Images { get; set; } = [];
    }

    public class VehicleImageDto
    {
        public long Id { get; set; }
        public string? Label { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public long? EventId { get; set; }
    }
}
