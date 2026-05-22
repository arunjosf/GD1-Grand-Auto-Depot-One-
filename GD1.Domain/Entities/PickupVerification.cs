using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;
using System;

namespace GD1.Domain.Entities
{
    public class PickupVerification : BaseEntity
    {
        public long BookingId { get; set; }
        public long ManagerId { get; set; }
        
        public ReportType Type { get; set; }

        // Mandatory Labeled Images
        public string FrontImageUrl { get; set; } = string.Empty;
        public string RearImageUrl { get; set; } = string.Empty;
        public string LeftSideImageUrl { get; set; } = string.Empty;
        public string RightSideImageUrl { get; set; } = string.Empty;
        public string InteriorImageUrl { get; set; } = string.Empty;
        public string OdometerImageUrl { get; set; } = string.Empty;
        public string? SelfieUrl { get; set; }

        public string? IdProofUrl { get; set; }
        public string? RegistrationDocUrl { get; set; }
        
        public string? ManagerRemarks { get; set; }
        public DateTime VerifiedAt { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
