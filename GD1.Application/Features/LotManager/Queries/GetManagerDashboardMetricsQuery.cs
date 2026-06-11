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
    }

    public class ManagerVehicleDetailDto : ManagerVehicleDto
    {
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
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
