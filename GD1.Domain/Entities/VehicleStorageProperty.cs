using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class VehicleStorageProperty : BaseEntity
    {
        public long LotOwnerId { get; set; }
        public string LotCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public string Status { get; set; } = "Active";
        public bool IsHidden { get; set; } = false;
        public bool IsBlocked { get; set; } = false;

        // Property-level facilities (Since slots are just garages)
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshopBay { get; set; }
        public bool HasWashingArea { get; set; }
        public string? ExtraFacilities { get; set; }
        
        public decimal PricePerDay { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public User LotOwner { get; set; } = null!;
        
        // Direct Slots (Garages)
        public ICollection<VehicleStorageSlot> Slots { get; set; } = [];
        
        public ICollection<LotManager> Managers { get; set; } = [];
        public ICollection<Booking> Bookings { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<PropertyImage> ActivePropertyImages { get; set; } = [];
    }
}
