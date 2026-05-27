using GD1.Domain.Entities.Base;
using System;

namespace GD1.Domain.Entities
{
    public class StoredVehicle : BaseEntity
    {
        public long PropertyId { get; set; }
        public long SlotId { get; set; }
        public long VehicleId { get; set; }
        public long BookingId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public VehicleStorageProperty Property { get; set; } = null!;
        public VehicleStorageSlot Slot { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}
