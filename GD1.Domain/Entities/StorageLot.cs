using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class StorageLot : BaseEntity
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

        public int TotalSlots { get; set; }
        public string Tier { get; set; } = "Tier1";
        public string Status { get; set; } = "PendingReview";
        public bool HasCCTV { get; set; }
        public bool HasWorkshopBay { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasSecurity { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public User LotOwner { get; set; } = null!;
        public ICollection<LotSlot> Slots { get; set; } = [];
        public ICollection<LotManager> Managers { get; set; } = [];
        public ICollection<Booking> Bookings { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];

    }
}
