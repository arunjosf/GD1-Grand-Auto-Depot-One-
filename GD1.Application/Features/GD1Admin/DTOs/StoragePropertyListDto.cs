using System;
using System.Collections.Generic;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class StoragePropertyListDto
    {
        public long Id { get; set; }
        public string LotCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Tier { get; set; } = "Private Garage";
        public int TotalSlots { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double DistanceKm { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public LotContactDto ContactInfo { get; set; } = new();
        public LotPropertyDetailDto PropertyDetails { get; set; } = new();
        public bool HasCompatibleSlots { get; set; } = true;
        public bool IsPickupAvailable { get; set; } = false;
        
        // Direct Slots for the Rectangle UI
        public List<LotSlotDto> Slots { get; set; } = [];
        public List<string> PropertyImages { get; set; } = [];
    }

    public class LotContactDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public string? OwnerPhone { get; set; }
    }

    public class LotPropertyDetailDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }
        public string? ExtraFacilities { get; set; }
    }

    public class LotSlotDto
    {
        public long Id { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public string? ImageUrl { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public bool IsCompatible { get; set; } = true;
    }
}
