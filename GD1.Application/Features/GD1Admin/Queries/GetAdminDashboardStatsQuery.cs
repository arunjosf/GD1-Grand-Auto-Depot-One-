using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
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
        public int TotalServiceBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalPartneredGarages { get; set; }
        public int TotalServiceCenters { get; set; }
        public string MostBookedLotName { get; set; } = string.Empty;

        // Monthly stats (last 12 months)
        public List<MonthlyStatDto> MonthlyStats { get; set; } = new();

        // Yearly stats (last 3 years)
        public List<YearlyStatDto> YearlyStats { get; set; } = new();

        // Top 5 garages by booking count
        public List<TopGarageDto> TopGarages { get; set; } = new();

        // Top 5 service centers by service booking count
        public List<TopServiceCenterDto> TopServiceCenters { get; set; } = new();
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = string.Empty;
        public int GarageBookings { get; set; }
        public int ServiceBookings { get; set; }
        public decimal GarageRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int TotalSales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class YearlyStatDto
    {
        public string Year { get; set; } = string.Empty;
        public int GarageBookings { get; set; }
        public int ServiceBookings { get; set; }
        public decimal GarageRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
    }

    public class TopGarageDto
    {
        public long PropertyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopServiceCenterDto
    {
        public long ServiceCenterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalServiceBookings { get; set; }
        public decimal TotalRevenue { get; set; }
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
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _serviceCenterRepo;
        private readonly IGenericRepository<Payment> _paymentRepo;

        public GetAdminDashboardStatsQueryHandler(
            IGenericRepository<User> userRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<StoredVehicle> storedVehicleRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> franchiseRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> serviceCenterRepo,
            IGenericRepository<Payment> paymentRepo)
        {
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _storedVehicleRepo = storedVehicleRepo;
            _serviceRequestRepo = serviceRequestRepo;
            _franchiseRepo = franchiseRepo;
            _propertyRepo = propertyRepo;
            _serviceCenterRepo = serviceCenterRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<BaseResponse<AdminDashboardStatsDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepo.GetAllAsync();
            var bookings = await _bookingRepo.GetAllAsync();
            var storedVehicles = await _storedVehicleRepo.FindAsync(sv => sv.IsActive);
            var serviceJobs = await _serviceRequestRepo.GetAllAsync();
            var apps = await _franchiseRepo.GetAllAsync();
            var properties = await _propertyRepo.GetAllAsync();
            var serviceCenters = await _serviceCenterRepo.GetAllAsync();

            // Only "paid" payments — these are real completed transactions
            var paidPayments = (await _paymentRepo.FindAsync(p => p.Status == "paid")).ToList();

            // Admin's real garage revenue = AdminCutAmount on each paid payment
            var totalGarageAdminCut = paidPayments.Sum(p => p.AdminCutAmount);

            // Admin's real service revenue = PlatformFee on each paid service request
            var paidServiceJobs = serviceJobs.Where(s => s.IsPaid).ToList();
            var totalServiceAdminCut = paidServiceJobs.Sum(s => s.PlatformFee);

            var totalAdminRevenue = totalGarageAdminCut + totalServiceAdminCut;

            // Build a lookup: BookingId → Payment (paid)
            var paidPaymentByBooking = paidPayments.ToDictionary(p => p.BookingId, p => p);

            // ---- Top 5 Garages (by paid bookings) ----
            var topGarages = paidPayments
                .Join(bookings, p => p.BookingId, b => b.Id, (p, b) => new { p, b })
                .GroupBy(x => x.b.PropertyId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g =>
                {
                    var prop = properties.FirstOrDefault(p => p.Id == g.Key);
                    return new TopGarageDto
                    {
                        PropertyId = g.Key,
                        Name = prop?.Name ?? $"Garage #{g.Key}",
                        Location = prop?.AddressLine ?? "N/A",
                        TotalBookings = g.Count(),
                        TotalRevenue = g.Sum(x => x.p.AdminCutAmount)
                    };
                }).ToList();

            // ---- Top 5 Service Centers (by paid service requests) ----
            var topServiceCenters = paidServiceJobs
                .GroupBy(s => s.ServiceCenterId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g =>
                {
                    var sc = serviceCenters.FirstOrDefault(s => s.Id == g.Key);
                    return new TopServiceCenterDto
                    {
                        ServiceCenterId = g.Key,
                        Name = sc?.Name ?? $"Service Center #{g.Key}",
                        Location = sc?.AddressLine ?? "N/A",
                        TotalServiceBookings = g.Count(),
                        TotalRevenue = g.Sum(s => s.PlatformFee)
                    };
                }).ToList();

            // ---- Monthly stats (last 12 months) ----
            var monthlyStats = new List<MonthlyStatDto>();
            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = System.DateTime.UtcNow.AddMonths(-i);
                // Garage: paid payments in this month
                var monthPaidPayments = paidPayments
                    .Where(p => p.CreatedAt.Year == targetMonth.Year && p.CreatedAt.Month == targetMonth.Month)
                    .ToList();
                // Service: paid service requests in this month
                var monthPaidServices = paidServiceJobs
                    .Where(s => s.CreatedAt.Year == targetMonth.Year && s.CreatedAt.Month == targetMonth.Month)
                    .ToList();

                // Count garage bookings from payments (unique BookingIds)
                var monthGarageBookingIds = monthPaidPayments.Select(p => p.BookingId).Distinct().ToList();
                var garageRev = monthPaidPayments.Sum(p => p.AdminCutAmount);
                var svcRev = monthPaidServices.Sum(s => s.PlatformFee);

                monthlyStats.Add(new MonthlyStatDto
                {
                    Month = targetMonth.ToString("MMM yyyy"),
                    GarageBookings = monthGarageBookingIds.Count,
                    ServiceBookings = monthPaidServices.Count,
                    GarageRevenue = garageRev,
                    ServiceRevenue = svcRev,
                    TotalSales = monthGarageBookingIds.Count + monthPaidServices.Count,
                    Revenue = garageRev + svcRev
                });
            }

            // ---- Yearly stats (last 3 years) ----
            var yearlyStats = new List<YearlyStatDto>();
            for (int i = 2; i >= 0; i--)
            {
                var yr = System.DateTime.UtcNow.Year - i;
                var yrPaidPayments = paidPayments.Where(p => p.CreatedAt.Year == yr).ToList();
                var yrPaidServices = paidServiceJobs.Where(s => s.CreatedAt.Year == yr).ToList();
                yearlyStats.Add(new YearlyStatDto
                {
                    Year = yr.ToString(),
                    GarageBookings = yrPaidPayments.Select(p => p.BookingId).Distinct().Count(),
                    ServiceBookings = yrPaidServices.Count,
                    GarageRevenue = yrPaidPayments.Sum(p => p.AdminCutAmount),
                    ServiceRevenue = yrPaidServices.Sum(s => s.PlatformFee)
                });
            }

            // Most booked property (by paid payments)
            var mostBookedPropertyId = paidPayments
                .Join(bookings, p => p.BookingId, b => b.Id, (p, b) => b.PropertyId)
                .GroupBy(pid => pid)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var mostBookedProperty = properties.FirstOrDefault(p => p.Id == mostBookedPropertyId);

            var stats = new AdminDashboardStatsDto
            {
                TotalUsers = users.Count(),
                ActiveBookings = bookings.Count(b => b.Status == BookingStatus.InLot || b.Status == BookingStatus.AwaitingAgreement),
                VehiclesStored = storedVehicles.Count(),
                ServiceJobs = serviceJobs.Count(s => s.Status != "Cancelled"),
                TotalServiceBookings = paidServiceJobs.Count,
                TotalBookings = paidPayments.Select(p => p.BookingId).Distinct().Count(),
                TotalRevenue = totalAdminRevenue,
                NetProfit = totalAdminRevenue * 0.85m,
                MonthlyRevenue = totalAdminRevenue,
                TotalPartneredGarages = apps.Count(a => a.Status == FranchiseStatus.Approved && a.ApplicationType == ApplicationType.Franchise),
                TotalServiceCenters = apps.Count(a => a.Status == FranchiseStatus.Approved && a.ApplicationType == ApplicationType.ServiceCenter),
                PendingFranchiseApplications = apps.Count(a => a.Status == FranchiseStatus.Pending && a.ApplicationType == ApplicationType.Franchise),
                PendingServiceCenterApplications = apps.Count(a => a.Status == FranchiseStatus.Pending && a.ApplicationType == ApplicationType.ServiceCenter),
                MostBookedLotName = mostBookedProperty?.Name ?? "None",
                MonthlyStats = monthlyStats,
                YearlyStats = yearlyStats,
                TopGarages = topGarages,
                TopServiceCenters = topServiceCenters
            };

            return BaseResponse<AdminDashboardStatsDto>.Ok(stats);
        }
    }
}
