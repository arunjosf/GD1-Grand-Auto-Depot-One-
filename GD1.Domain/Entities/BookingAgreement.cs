using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;

namespace GD1.Domain.Entities
{
    public class BookingAgreement : BaseEntity
    {
        public long OwnerId { get; set; }
        public long VehicleId { get; set; }
        public long PropertyId { get; set; }
        public long? BookingId { get; set; }
        
        public string Content { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? PdfUrl { get; set; }
        
        public AgreementStatus Status { get; set; } = AgreementStatus.Pending;

        // Snapshots of data at time of signing
        public string VehicleSnapshotJson { get; set; } = "{}";
        public string LotSnapshotJson { get; set; } = "{}";

        public Booking? Booking { get; set; }
        public User Owner { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public VehicleStorageProperty Property { get; set; } = null!;
    }
}
