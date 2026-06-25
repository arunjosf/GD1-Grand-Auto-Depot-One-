using GD1.Domain.Entities.Base;
using System.Collections.Generic;

namespace GD1.Domain.Entities
{
    public class ServiceCenterPartneringApplication : BaseEntity
    {
        public long ApplicantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? PostalCode { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string Status { get; set; } = "PendingReview";
        
        public string? AdminNotes { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }

        public decimal ApplicationFee { get; set; } = 2000m;
        public string FeeStatus { get; set; } = "Pending";
        public string FeeTransactionId { get; set; } = string.Empty;
        public DateTime? PreferredInspectionDate { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsAiVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<ServiceCenterImage> Images { get; set; } = [];
    }
}
