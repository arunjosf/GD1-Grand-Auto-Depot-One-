using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveBookings { get; set; }
        public int VehiclesStored { get; set; }
        public int ServiceJobs { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingFranchiseApplications { get; set; }
        public int PendingServiceCenterApplications { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalPartneredGarages { get; set; }
        public int TotalServiceCenters { get; set; }
        public string MostBookedLotName { get; set; } = string.Empty;
        public System.Collections.Generic.List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalSales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class GetAdminDashboardStatsQuery : IRequest<BaseResponse<AdminDashboardStatsDto>>
    {
    }

    public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, BaseResponse<AdminDashboardStatsDto>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<StoredVehicle> _storedVehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _franchiseRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public GetAdminDashboardStatsQueryHandler(
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<StoredVehicle> storedVehicleRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> franchiseRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _storedVehicleRepo = storedVehicleRepo;
            _serviceRequestRepo = serviceRequestRepo;
            _franchiseRepo = franchiseRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<AdminDashboardStatsDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepo.GetAllAsync();
            var bookings = await _bookingRepo.GetAllAsync();
            var storedVehicles = await _storedVehicleRepo.FindAsync(sv => sv.IsActive);
            var serviceJobs = await _serviceRequestRepo.GetAllAsync();
            var apps = await _franchiseRepo.GetAllAsync();
            var properties = await _propertyRepo.GetAllAsync();

            var totalRevenue = bookings.Sum(b => b.TotalCost);

            // Find most booked lot
            var mostBookedPropertyId = bookings.GroupBy(b => b.PropertyId)
                                               .OrderByDescending(g => g.Count())
                                               .Select(g => g.Key)
                                               .FirstOrDefault();
            var mostBookedProperty = properties.FirstOrDefault(p => p.Id == mostBookedPropertyId);

            // Monthly stats (last 6 months)
            var monthlyStats = new System.Collections.Generic.List<MonthlyStatDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = System.DateTime.UtcNow.AddMonths(-i);
                var monthlyBookings = bookings.Where(b => b.CreatedAt.Year == targetMonth.Year && b.CreatedAt.Month == targetMonth.Month).ToList();
                monthlyStats.Add(new MonthlyStatDto
                {
                    Month = targetMonth.ToString("MMM yyyy"),
                    TotalSales = monthlyBookings.Count,
                    Revenue = monthlyBookings.Sum(b => b.TotalCost)
                });
            }

            var stats = new AdminDashboardStatsDto
            {
                TotalUsers = users.Count(),
                ActiveBookings = bookings.Count(b => b.Status == BookingStatus.InLot || b.Status == BookingStatus.AwaitingAgreement),
                VehiclesStored = storedVehicles.Count(),
                ServiceJobs = serviceJobs.Count(s => s.Status != "Cancelled"),
                MonthlyRevenue = totalRevenue * 0.1m, // Platform fee logic
                TotalBookings = bookings.Count(),
                TotalRevenue = totalRevenue,
                NetProfit = totalRevenue * 0.35m, // Based on requested mockup logic
                TotalPartneredGarages = apps.Count(a => a.Status == FranchiseStatus.Approved && a.ApplicationType == ApplicationType.Franchise),
                TotalServiceCenters = apps.Count(a => a.Status == FranchiseStatus.Approved && a.ApplicationType == ApplicationType.ServiceCenter),
                PendingFranchiseApplications = apps.Count(a => a.Status == FranchiseStatus.Pending && a.ApplicationType == ApplicationType.Franchise),
                PendingServiceCenterApplications = apps.Count(a => a.Status == FranchiseStatus.Pending && a.ApplicationType == ApplicationType.ServiceCenter),
                MostBookedLotName = mostBookedProperty?.Name ?? "None",
                MonthlyStats = monthlyStats
            };

            return BaseResponse<AdminDashboardStatsDto>.Ok(stats);
        }
    }
}
