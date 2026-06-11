using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.DTOs
{
    public class BookingDto
    {
        public long Id { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Invalid VehicleId.")]
        public long VehicleId { get; set; }

        [Required]
        [StringLength(100)]
        public string VehicleBrand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string VehicleModel { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RegistrationNo { get; set; } = string.Empty;

        [Range(1, long.MaxValue, ErrorMessage = "Invalid PropertyId.")]
        public long PropertyId { get; set; }

        [Required]
        [StringLength(150)]
        public string PropertyName { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string PropertyAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SlotNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be non-negative.")]
        public decimal TotalCost { get; set; }

        public decimal PricePerDay { get; set; }

        public DateTime CreatedAt { get; set; }
        public int IsAgreementSigned { get; set; }
        
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public string? PropertyImageUrl { get; set; }
        public string? VehicleImageUrl { get; set; }
        public string? VehicleRcUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? OwnerName { get; set; }
        public string? LotOwnerName { get; set; }
        public string? LotOwnerPhone { get; set; }

        // Pickup / Manager Details
        public string? PickupStatus { get; set; }
        public DateTime? ManagerArrivalTime { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerPhone { get; set; }
        public string? ManagerSelfieUrl { get; set; }
        public string? ManagerIdProofUrl { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public double? LotLatitude { get; set; }
        public double? LotLongitude { get; set; }
        public double? LastGpsLatitude { get; set; }
        public double? LastGpsLongitude { get; set; }


        // --- INTERNAL FLAT FIELDS FROM DB ---
        [JsonIgnore] public string? FrontImageUrl { get; set; }
        [JsonIgnore] public string? RearImageUrl { get; set; }
        [JsonIgnore] public string? LeftSideImageUrl { get; set; }
        [JsonIgnore] public string? RightSideImageUrl { get; set; }
        [JsonIgnore] public string? SelfieUrl { get; set; }
        [JsonIgnore] public string? InteriorImageUrl { get; set; }
        [JsonIgnore] public string? OdometerImageUrl { get; set; }
        [JsonIgnore] public string? ManagerRemarks { get; set; }

        [JsonIgnore] public string? ArrivalFrontImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalRearImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalLeftSideImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalRightSideImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalInteriorImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalOdometerImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalManagerRemarks { get; set; }

        // --- NESTED JSON OBJECTS ---
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionReportDto? PickupImages => 
            (string.IsNullOrWhiteSpace(FrontImageUrl) && string.IsNullOrWhiteSpace(InteriorImageUrl)) ? null : new ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(FrontImageUrl) ? null : FrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(RearImageUrl) ? null : RearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(LeftSideImageUrl) ? null : LeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(RightSideImageUrl) ? null : RightSideImageUrl,
                SelfieUrl = string.IsNullOrWhiteSpace(SelfieUrl) ? null : SelfieUrl,
                InteriorImageUrl = string.IsNullOrWhiteSpace(InteriorImageUrl) ? null : InteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(OdometerImageUrl) ? null : OdometerImageUrl,
                ManagerRemarks = string.IsNullOrWhiteSpace(ManagerRemarks) ? null : ManagerRemarks
            };

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionReportDto? ArrivalImages => 
            string.IsNullOrEmpty(ArrivalFrontImageUrl) ? null : new ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(ArrivalFrontImageUrl) ? null : ArrivalFrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(ArrivalRearImageUrl) ? null : ArrivalRearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(ArrivalLeftSideImageUrl) ? null : ArrivalLeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(ArrivalRightSideImageUrl) ? null : ArrivalRightSideImageUrl,
                SelfieUrl = null,
                InteriorImageUrl = string.IsNullOrWhiteSpace(ArrivalInteriorImageUrl) ? null : ArrivalInteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(ArrivalOdometerImageUrl) ? null : ArrivalOdometerImageUrl,
                ManagerRemarks = string.IsNullOrWhiteSpace(ArrivalManagerRemarks) ? null : ArrivalManagerRemarks
            };
    }

    public class ConditionReportDto
    {
        public string? FrontImageUrl { get; set; }
        public string? RearImageUrl { get; set; }
        public string? LeftSideImageUrl { get; set; }
        public string? RightSideImageUrl { get; set; }
        public string? SelfieUrl { get; set; }
        public string? InteriorImageUrl { get; set; }
        public string? OdometerImageUrl { get; set; }
        public string? ManagerRemarks { get; set; }
    }
}
