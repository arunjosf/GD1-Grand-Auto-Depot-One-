using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Domain.Entities.Base;

namespace GD1.Domain.Entities
{
    public class FranchiseApplication : BaseEntity
    {
        public long ApplicantId { get; set; }

        public string ApplicationType { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? PropertyProofUrl { get; set; }

        public decimal ApplicationFee { get; set; } = 2000;

        public string FeeStatus { get; set; } = "Pending";
        public string? FeeTransactionId { get; set; }

        public string Status { get; set; } = "Pending";
        public string? AdminNotes { get; set; }
        public long? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public User Applicant { get; set; } = null!;
        public ICollection<LotUnit> LotUnits { get; set; } = [];
        public ICollection<InspectionReport> InspectionReports { get; set; } = [];
        public ICollection<PropertyImage> PropertyImages { get; set; } = [];
    }
}
