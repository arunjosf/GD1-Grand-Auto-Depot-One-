using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Base;
using GD1.Domain.Entities.Enums;

namespace GD1.Domain.Entities
{
    public class FranchiseApplication : BaseEntity
    {
        public long ApplicantId { get; set; }

        public ApplicationType ApplicationType { get; set; } = ApplicationType.Franchise;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? PostalCode { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? PreferredInspectionDate { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? PropertyProofUrl { get; set; }
        
        // Service Center fields
        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrands { get; set; }

        public decimal PricePerDay { get; set; }

        public decimal ApplicationFee { get; set; } = 2000;
        public string FeeStatus { get; set; } = "Pending";
        public string? FeeTransactionId { get; set; }

        public FranchiseStatus Status { get; set; } = FranchiseStatus.Pending;
        public bool IsAiVerified { get; set; }
        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
        public long? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Application-level facility declarations
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }

        public User Applicant { get; set; } = null!;
        
        // Direct Slots (Garages) proposed by owner
        public ICollection<FranchiseSlot> Slots { get; set; } = [];
        
        public ICollection<InspectionAssignment> Assignments { get; set; } = [];
        public ICollection<PropertyImage> PropertyImages { get; set; } = [];
    }
}

