using System;
using System.Collections.Generic;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.GD1Admin.DTOs
{
    public class AdminApplicationDto
    {
        public long Id { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public FranchiseStatus Status { get; set; } = FranchiseStatus.Pending;
        public bool IsAiVerified { get; set; }
        public string? AdminNotes { get; set; }
        public decimal ApplicationFee { get; set; }
        public decimal PricePerDay { get; set; }
        public string FeeStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PreferredInspectionDate { get; set; }
        
        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }
        public bool HasFireSafety { get; set; }

        public string PropertyFrontImageUrl { get; set; } = string.Empty;
        public List<string> OtherImageUrls { get; set; } = [];

        public AdminAgentSummaryDto? AssignedAgent { get; set; }
        public AdminInspectionReportDto? InspectionReport { get; set; }

        // Direct Slots (Garages)
        public List<AdminFranchiseSlotDto> Slots { get; set; } = [];
        public List<InspectionAssignmentDto> Assignments { get; set; } = [];

        // Service Center Specific (Nullable)
        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrand { get; set; }
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

    public class AdminInspectionReportDto
    {
        public long Id { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? OverallDescription { get; set; }
        public List<AdminInspectionSlotDto> SlotVerifications { get; set; } = [];
        public List<AdminPropertyImageDto> SiteImages { get; set; } = [];
    }

    public class AdminInspectionSlotDto
    {
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class AdminFranchiseSlotDto
    {
        public long Id { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? ImageUrl { get; set; }
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
