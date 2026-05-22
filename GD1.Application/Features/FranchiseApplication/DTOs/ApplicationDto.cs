using GD1.Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class ApplicationDto
    {
        public long Id { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationType ApplicationType { get; set; }
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
        public GD1.Domain.Entities.Enums.FranchiseStatus? Status { get; set; }

        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrand { get; set; }
        public bool IsAiVerified { get; set; }
        public string? AdminNotes { get; set; }
        public decimal ApplicationFee { get; set; }
        public decimal PricePerDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PreferredInspectionDate { get; set; }

        public string FrontImageUrl { get; set; } = string.Empty;
        public string? OtherImageUrls { get; set; } // Legacy or flattened
        public string FeeStatus { get; set; } = string.Empty;

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }

        public List<PropertyImageDto> PropertyImages { get; set; } = [];
        public List<FranchiseSlotDto> Slots { get; set; } = [];
        public List<InspectionAssignmentDto> Assignments { get; set; } = [];
    }

    public class UserApplicationDto
    {
        public long Id { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationType ApplicationType { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public DateTime? PreferredInspectionDate { get; set; }
        public FranchiseStatus Status { get; set; } = FranchiseStatus.Pending;
        public string? AdminNotes { get; set; }
        public decimal ApplicationFee { get; set; }
        public decimal PricePerDay { get; set; }
        public string FeeStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FrontImageUrl { get; set; } = string.Empty;

        public List<FranchiseSlotDto> Slots { get; set; } = [];
    }

    public class FranchiseSlotDto
    {
        public long Id { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class InspectionAssignmentDto
    {
        public long Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AgentName { get; set; }
        public FranchiseInspectionReportDto? Report { get; set; }
    }

    public class FranchiseInspectionReportDto
    {
        public long Id { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? OverallDescription { get; set; }
        public List<InspectionSlotVerificationDto> SlotVerifications { get; set; } = [];
        public List<PropertyImageDto> SiteImages { get; set; } = [];
    }

    public class InspectionSlotVerificationDto
    {
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class PropertyImageDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public bool IsMain { get; set; }
    }

    // Submission DTOs for the Agent
    public class PropertyInspectionSubmission
    {
        public string OverallDescription { get; set; } = string.Empty;
        public List<string> SiteImages { get; set; } = [];
        public List<SlotInspectionSubmission> Slots { get; set; } = [];
    }

    public class SlotInspectionSubmission
    {
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string? ImageUrl { get; set; }
    }
}
