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
        public Guid VehicleId { get; set; }
        public Guid? BookingId { get; set; }
        public string EventType { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? PhotoUrls { get; set; } 
        public Guid? TriggeredBy { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}
