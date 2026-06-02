using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Base;

namespace GD1.Domain.Entities
{
    public class ServiceRequest : BaseEntity
    {
        public long BookingId { get; set; }
        public long ServiceCenterId { get; set; }
        public long? MechanicId { get; set; }
        public long RequestedBy { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";

        public DateTime? ScheduledDate { get; set; }
        public string? CancellationReason { get; set; }
        public bool? IsCompleted { get; set; }
        public string? CompletionNotes { get; set; }
        public string? CompletionPhotos { get; set; }
        public string? BillUrl { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal CenterEarning { get; set; }

        public string? MechanicEmail { get; set; }
        public string? MechanicOtp { get; set; }

        public Booking Booking { get; set; } = null!;
        public ServiceCenter ServiceCenter { get; set; } = null!;
        public Mechanics? Mechanic { get; set; }

    }
}
