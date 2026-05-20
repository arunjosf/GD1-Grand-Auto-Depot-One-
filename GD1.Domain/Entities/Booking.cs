using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public long VehicleId { get; set; }
        public long PropertyId { get; set; }
        public long? SlotId { get; set; }
        public long OwnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? PickupPincode { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public decimal PricePerDay { get; set; }
        public decimal TotalCost { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal LotEarning { get; set; }
        public string? CCTVUrl { get; set; }

        public string? PickupAddress { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public bool IsPickupRequested { get; set; }
        public long? AssignedManagerId { get; set; }
        public string? PickupOtp { get; set; }
        public bool IsAgreementSigned { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
        public VehicleStorageProperty Property { get; set; } = null!;
        public VehicleStorageSlot? Slot { get; set; }
        public User Owner { get; set; } = null!;
        public ICollection<Handoff> Handoffs { get; set; } = [];
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
        public ICollection<VehicleJourneyEvent> JourneyEvents { get; set; } = [];
    }
}
