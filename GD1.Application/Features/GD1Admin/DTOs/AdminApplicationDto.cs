using System;
using System.Collections.Generic;
using GD1.Application.Features.FranchiseApplication.DTOs;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class AdminApplicationDto
    {
        public long Id { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        // Location Info
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Status & Pricing
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public decimal ApplicationFee { get; set; }
        public string FeeStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PreferredInspectionDate { get; set; }
        
        // Re-application logic
        public bool IsReapplication { get; set; }
        public List<PastRejectionDto> RejectionHistory { get; set; } = [];

        // Property Visuals
        public string PropertyFrontImageUrl { get; set; } = string.Empty;
        public List<string> OtherImageUrls { get; set; } = [];
        public List<string> ExtraFacilities { get; set; } = [];

        // Agent & Inspection Info (Populated based on Status)
        public AdminAgentSummaryDto? AssignedAgent { get; set; }
        public InspectionReportDto? InspectionReport { get; set; }

        // Nested Data
        public List<AdminLotUnitDto> LotUnits { get; set; } = [];
        public List<InspectionAssignmentDto> Assignments { get; set; } = [];
    }

    public class AdminAgentSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? SelfieUrl { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class AdminLotUnitDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }
        public List<string> ExtraFacilities { get; set; } = [];
        public string Status { get; set; } = string.Empty;

        // Renamed from OwnerImages
        public List<AdminPropertyImageDto> LotImages { get; set; } = [];
    }

    public class AdminPropertyImageDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }
}
