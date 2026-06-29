using GD1.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Domain.Entities
{
    public class ServiceCenter : BaseEntity
    {
        public long AdminId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? PostalCode { get; set; }
        public string District { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string Status { get; set; } = "PendingReview";
        public int CoverageRadiusKm { get; set; } = 20;
        public decimal AverageRating { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsHidden { get; set; } = false;
        public bool IsBlocked { get; set; } = false;

        public string? BusinessRegistrationUrl { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string? OwnerIdProofUrl { get; set; }
        public string? AdminNotes { get; set; }

        public User ServiceCenterAdmin { get; set; } = null!;
        public ICollection<Mechanics> Mechanics { get; set; } = [];
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
        
        public ICollection<ServiceCenterImage> Images { get; set; } = [];
    }
}
