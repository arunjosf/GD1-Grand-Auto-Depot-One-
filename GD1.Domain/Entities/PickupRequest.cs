using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Enums;

namespace GD1.Domain.Entities
{
    public class PickupRequest : BaseEntity
    {
        public long BookingId { get; set; }
        public long? ManagerId { get; set; }
        public DateTime? RequestedPickupTime { get; set; }
        public DateTime? ManagerArrivalTime { get; set; }
        public bool IsApprovedByLotOwner { get; set; }
        public string? OtpHash { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public bool IsOtpVerified { get; set; }

        public PickupStatus Status { get; set; } = PickupStatus.Requested;
        public Booking Booking { get; set; } = null!;
        public LotManager? Manager { get; set; }
    }
}

