using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetManagerDashboardMetricsQuery : IRequest<BaseResponse<ManagerDashboardMetricsDto>>
    {
        public long ManagerId { get; set; }
    }

    public class ManagerDashboardMetricsDto
    {
        public int TotalVehicles { get; set; }
        public int PendingPickupsCount { get; set; }
        public int UpcomingServicesCount { get; set; }
        public int PendingOnDemandCount { get; set; }
        public int PendingWeeklyCount { get; set; }
        public List<PerformanceGraphItemDto> PerformanceGraphData { get; set; } = new();
    }

    public class PerformanceGraphItemDto
    {
        public string Date { get; set; } = string.Empty;
        public int PickupsDone { get; set; }
        public int OnDemandImagesDone { get; set; }
        public int WeeklySubmissionsDone { get; set; }
    }

    public class ManagerPickupDto
    {
        public long PickupRequestId { get; set; }
        public long BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public System.DateTime RequestedPickupTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string VehicleImage { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
    }

    public class ManagerVehicleDto
    {
        public long VehicleId { get; set; }
        public long BookingId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public System.DateTime StoredSince { get; set; }
        public bool HasPendingOnDemandRequest { get; set; }
    }

    public class ManagerVehicleDetailDto : ManagerVehicleDto
    {
        public long OwnerId { get; set; }
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public long LotOwnerId { get; set; }
        public string LotOwnerName { get; set; } = string.Empty;
        public string LotOwnerPhone { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime? LastOnDemandImageDate { get; set; }
        public DateTime? LastServiceReportDate { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public bool HasServiceRecommendation { get; set; }
        public string? ManagerServiceRemarks { get; set; }

        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandFrontImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandRearImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandLeftSideImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandRightSideImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandInteriorImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? OnDemandOdometerImageUrl { get; set; }

        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateFrontImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateRearImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateLeftSideImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateRightSideImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateInteriorImageUrl { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? WeeklyUpdateOdometerImageUrl { get; set; }

        public string? WeeklyUpdateDescription { get; set; }
        public DateTime? LastWeeklyUpdateDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnoreAttribute(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public GD1.Application.Features.LotBooking.DTOs.ConditionReportDto? RecentOnDemandImages =>
            string.IsNullOrEmpty(OnDemandFrontImageUrl) ? null : new GD1.Application.Features.LotBooking.DTOs.ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(OnDemandFrontImageUrl) ? null : OnDemandFrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(OnDemandRearImageUrl) ? null : OnDemandRearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(OnDemandLeftSideImageUrl) ? null : OnDemandLeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(OnDemandRightSideImageUrl) ? null : OnDemandRightSideImageUrl,
                InteriorImageUrl = string.IsNullOrWhiteSpace(OnDemandInteriorImageUrl) ? null : OnDemandInteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(OnDemandOdometerImageUrl) ? null : OnDemandOdometerImageUrl
            };

        [System.Text.Json.Serialization.JsonIgnoreAttribute(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public GD1.Application.Features.LotBooking.DTOs.ConditionReportDto? RecentWeeklyUpdateImages =>
            string.IsNullOrEmpty(WeeklyUpdateFrontImageUrl) ? null : new GD1.Application.Features.LotBooking.DTOs.ConditionReportDto
            {
                FrontImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateFrontImageUrl) ? null : WeeklyUpdateFrontImageUrl,
                RearImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateRearImageUrl) ? null : WeeklyUpdateRearImageUrl,
                LeftSideImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateLeftSideImageUrl) ? null : WeeklyUpdateLeftSideImageUrl,
                RightSideImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateRightSideImageUrl) ? null : WeeklyUpdateRightSideImageUrl,
                InteriorImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateInteriorImageUrl) ? null : WeeklyUpdateInteriorImageUrl,
                OdometerImageUrl = string.IsNullOrWhiteSpace(WeeklyUpdateOdometerImageUrl) ? null : WeeklyUpdateOdometerImageUrl
            };
    }

    public class GetManagerDashboardMetricsQueryHandler : IRequestHandler<GetManagerDashboardMetricsQuery, BaseResponse<ManagerDashboardMetricsDto>>
    {
        private readonly IManagerReadRepository _repo;
        public GetManagerDashboardMetricsQueryHandler(IManagerReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<ManagerDashboardMetricsDto>> Handle(GetManagerDashboardMetricsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetDashboardMetricsAsync(request.ManagerId);
            return BaseResponse<ManagerDashboardMetricsDto>.Ok(data);
        }
    }
}
