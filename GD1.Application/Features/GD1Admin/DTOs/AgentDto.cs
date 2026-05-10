using System;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class AgentDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string CoverageArea { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceKm { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        
        public string? SelfieUrl { get; set; }
        public string? IdProofUrl { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
    }
}
