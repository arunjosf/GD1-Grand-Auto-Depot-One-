using GD1.Domain.Entities.Base;
using System;

namespace GD1.Domain.Entities
{
    public class PickupVerification : BaseEntity
    {
        public long BookingId { get; set; }
        public long ManagerId { get; set; }
        
        // Mandatory Labeled Images
        public string FrontImageUrl { get; set; } = string.Empty;
        public string RearImageUrl { get; set; } = string.Empty;
        public string LeftSideImageUrl { get; set; } = string.Empty;
        public string RightSideImageUrl { get; set; } = string.Empty;
        public string InteriorImageUrl { get; set; } = string.Empty;
        public string EngineBayImageUrl { get; set; } = string.Empty; // Adding one more for professionalism

        public string? IdProofUrl { get; set; }
        public string? RegistrationDocUrl { get; set; }
        
        public string? ManagerRemarks { get; set; }
        public DateTime VerifiedAt { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
