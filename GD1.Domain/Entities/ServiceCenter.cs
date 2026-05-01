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
        public Guid AdminId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string Status { get; set; } = "PendingReview";
        public int CoverageRadiusKm { get; set; } = 20;
        public decimal AverageRating { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public User ServiceCenterAdmin { get; set; } = null;
        public ICollection<Mechanics> Mechanics { get; set; } = [];
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];

    }
}
