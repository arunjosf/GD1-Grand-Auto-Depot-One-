using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Booking : BaseEntity   
    {
        public Guid VehicleId { get; set; }
        public Guid LotId { get; set; }
        public Guid SlotId { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Plan { get; set; } = "Basic";

        public string Status { get; set; } = "Pending";

        public decimal TotalCost { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal LotEarning { get; set; }
        public string? CCTVUrl { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
        public StorageLot Lot { get; set; } = null!;
        public LotSlot Slot { get; set; } = null!;
        public User Owner { get; set; } = null!;
        public ICollection<VehicleJourneyEvent> JourneyEvents { get; set; } = [];
        public ICollection<Handoff> Handoffs { get; set; } = [];
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
    }
}
