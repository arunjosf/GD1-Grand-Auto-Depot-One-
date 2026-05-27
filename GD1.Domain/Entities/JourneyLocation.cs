using GD1.Domain.Entities.Base;
using System;

namespace GD1.Domain.Entities
{
    public class JourneyLocation : BaseEntity
    {
        public long BookingId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
