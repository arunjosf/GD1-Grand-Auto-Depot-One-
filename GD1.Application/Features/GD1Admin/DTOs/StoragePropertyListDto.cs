using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Tier { get; set; } = string.Empty;
        public int TotalSlots { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal AverageRating { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public List<string> ExtraFacilities { get; set; } = [];

        // Property Details
        public string AddressLine { get; set; } = string.Empty;
        public string FrontImageUrl { get; set; } = string.Empty;
        public List<string> OtherImageUrls { get; set; } = [];
        
        // Specific Lot Unit Details & Images
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }
        public int Capacity { get; set; }
        public string UnitLabel { get; set; } = string.Empty;
        public List<string> UnitImages { get; set; } = [];
    }
}
