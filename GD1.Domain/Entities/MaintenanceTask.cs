using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;

namespace GD1.Domain.Entities
{
    public class MaintenanceTask : BaseEntity
    {
        public long VehicleId { get; set; }
        public long BookingId { get; set; }
        public long ManagerId { get; set; }

        public MaintenanceTaskType Type { get; set; }
        public MaintenanceTaskStatus Status { get; set; } = MaintenanceTaskStatus.Pending;

        // Weekly Specific Fields
        public bool? CarWashCompleted { get; set; }
        public bool? TyrePressureChecked { get; set; }
        public bool? DailyStartupsCompleted { get; set; }
        public string? ManagerRemarks { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public LotManager Manager { get; set; } = null!;
    }
}
