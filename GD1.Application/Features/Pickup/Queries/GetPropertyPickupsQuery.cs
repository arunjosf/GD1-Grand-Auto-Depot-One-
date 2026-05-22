using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Queries
{
    public class GetPropertyPickupsQuery : IRequest<BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        public long PropertyId { get; set; }
        public long? ManagerId { get; set; }
    }

    public class PickupRequestDto
    {
        public long PickupRequestId { get; set; }
        public long BookingId { get; set; }
        public DateTime RequestedPickupTime { get; set; }
        public string Status { get; set; } = string.Empty;

        // Vehicle Info
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        // Location Info
        // Location Info
        public string? PickupAddress { get; set; }
        public string? PickupPincode { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public string? OwnerSubmittedOtp { get; set; }

        // --- INTERNAL FLAT FIELDS FROM DB ---
        [JsonIgnore] public string? FrontImageUrl { get; set; }
        [JsonIgnore] public string? RearImageUrl { get; set; }
        [JsonIgnore] public string? LeftSideImageUrl { get; set; }
        [JsonIgnore] public string? RightSideImageUrl { get; set; }
        [JsonIgnore] public string? SelfieUrl { get; set; }
        [JsonIgnore] public string? InteriorImageUrl { get; set; }
        [JsonIgnore] public string? OdometerImageUrl { get; set; }

        [JsonIgnore] public string? ArrivalFrontImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalRearImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalLeftSideImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalRightSideImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalInteriorImageUrl { get; set; }
        [JsonIgnore] public string? ArrivalOdometerImageUrl { get; set; }

        // --- NESTED JSON OBJECTS ---
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionReportDto? PickupImages => 
            string.IsNullOrEmpty(FrontImageUrl) ? null : new ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(FrontImageUrl) ? null : FrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(RearImageUrl) ? null : RearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(LeftSideImageUrl) ? null : LeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(RightSideImageUrl) ? null : RightSideImageUrl,
                SelfieUrl = string.IsNullOrWhiteSpace(SelfieUrl) ? null : SelfieUrl,
                InteriorImageUrl = string.IsNullOrWhiteSpace(InteriorImageUrl) ? null : InteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(OdometerImageUrl) ? null : OdometerImageUrl
            };

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionReportDto? ArrivalImages => 
            string.IsNullOrEmpty(ArrivalFrontImageUrl) ? null : new ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(ArrivalFrontImageUrl) ? null : ArrivalFrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(ArrivalRearImageUrl) ? null : ArrivalRearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(ArrivalLeftSideImageUrl) ? null : ArrivalLeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(ArrivalRightSideImageUrl) ? null : ArrivalRightSideImageUrl,
                SelfieUrl = null, // Selfie is not taken at arrival
                InteriorImageUrl = string.IsNullOrWhiteSpace(ArrivalInteriorImageUrl) ? null : ArrivalInteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(ArrivalOdometerImageUrl) ? null : ArrivalOdometerImageUrl
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
    }

    public class GetPropertyPickupsQueryHandler : IRequestHandler<GetPropertyPickupsQuery, BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        private readonly IPickupReadRepository _repo;

        public GetPropertyPickupsQueryHandler(IPickupReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<PickupRequestDto>>> Handle(
            GetPropertyPickupsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetPropertyPickupsAsync(request.PropertyId, request.ManagerId);
            return BaseResponse<IEnumerable<PickupRequestDto>>.Ok(result);
        }
    }
}
