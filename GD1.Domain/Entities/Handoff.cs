using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Handoff : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid RequestedBy { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string PersonPhone { get; set; } = string.Empty;
        public string? PersonIdUrl { get; set; }
        public string? PersonPhotoUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Pending";

        public bool? OwnerApproved { get; set; }

        public Booking Booking { get; set; } = null!;
        public DamageReport? DamageReport { get; set; }

    }
}
