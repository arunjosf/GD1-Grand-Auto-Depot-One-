using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class VehicleJourneyEvent : BaseEntity
    {
        public long VehicleId { get; set; }
        public long? BookingId { get; set; }
        public string EventType { get; set; } = string.Empty;

        public string? Description { get; set; } 
        public long? TriggeredBy { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
        public Booking? Booking { get; set; }
        public ICollection<VehicleImage> Images { get; set; } = [];

    }
}
