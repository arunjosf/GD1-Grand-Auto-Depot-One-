using GD1.Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GD1.Application.Features.FranchiseApplication.DTOs
{
    public class ApplicationDto
    {
        public long Id { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
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
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? PreferredInspectionDate { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public FranchiseStatus? Status { get; set; }
        public string? AdminNotes { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public decimal ApplicationFee { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? FeeStatus { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime CreatedAt { get; set; }
        public string FrontImageUrl { get; set; } = string.Empty;
        public List<string> OtherImageUrls { get; set; } = [];

        public List<LotUnitDto> LotUnits { get; set; } = [];
        public List<InspectionAssignmentDto> Assignments { get; set; } = [];
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public List<PastRejectionDto>? PastRejections { get; set; } = [];
    }

    public class UserApplicationDto
    {
        public long Id { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
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
        public string FeeStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FrontImageUrl { get; set; } = string.Empty;
        public List<string> OtherImageUrls { get; set; } = [];

        public List<UserLotUnitDto> LotUnits { get; set; } = [];
    }

    public class PastRejectionDto
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class InspectionAssignmentDto
    {
        public long Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public long AgentId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? AgentName { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? AgentCity { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? AgentSelfieUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? AgentIdProofUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? PhoneNumber { get; set; }
        public FranchiseInspectionReportDto? Report { get; set; }
    }


    public class FranchiseInspectionReportDto
    {
        public long Id { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Discription Must contain Atleast 10 char")]
        public string? OverallDescription { get; set; }

        public InspectionDecision? AdminDecision { get; set; }
        public string? AdminRemarks { get; set; }
        public List<FranchiseInspectionItemDto> Items { get; set; } = [];
        public List<PropertyImageDto> PropertyImages { get; set; } = [];
    }

    public class FranchiseInspectionItemDto
    {
        public long Id { get; set; }
        public long LotUnitId { get; set; }
        public string LotLabel { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? Remarks { get; set; }
        public List<PropertyImageDto> UnitImages { get; set; } = [];
    }

    public class LotUnitDto
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
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public FranchiseStatus? Status { get; set; }
        public List<PropertyImageDto> OwnerImages { get; set; } = [];
        public List<PropertyImageDto> AgentImages { get; set; } = [];
    }

    public class UserLotUnitDto
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
        public FranchiseStatus Status { get; set; } = FranchiseStatus.Pending;
        public List<UserPropertyImageDto> OwnerImages { get; set; } = [];
    }

    public class PropertyImageDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }

    public class UserPropertyImageDto
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UnitInspectionSubmission
    {
        public long LotUnitId { get; set; }
        public bool IsVerified { get; set; }

        [Required]
        public string? Remarks { get; set; }

        [Required]
        public List<string> UnitImages { get; set; } = []; 
    }

    public class PropertyInspectionSubmission
    {
        public string OverallDescription { get; set; } = string.Empty;

        [Required]
        public List<string> PropertyImages { get; set; } = []; 
        public List<UnitInspectionSubmission> Units { get; set; } = [];
    }

    public class AgentInspectionSummaryDto
    {
        public long AssignmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
    }
}
