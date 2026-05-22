using System;
using System.Collections.Generic;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class AdminServiceCenterApplicationDto
    {
        public long Id { get; set; }
        public long AdminId { get; set; }
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

        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrand { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        
        public string? BrandVerifyUrl { get; set; }
        public string? GoogleMapVerifyUrl { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}
