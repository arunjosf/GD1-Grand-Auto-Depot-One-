using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotOwner.Queries
{
    public class GetLotOwnerDashboardMetricsQuery : IRequest<BaseResponse<LotOwnerDashboardMetricsDto>>
    {
        public long LotOwnerId { get; set; }
    }

    public class LotOwnerDashboardMetricsDto
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public double CustomerRetentionRate { get; set; }
        
        public List<MonthlyBookingDataDto> MonthlyBookings { get; set; } = new();
        public List<YearlyBookingDataDto> YearlyBookings { get; set; } = new();
        
        public BestManagerDto? BestPerformedManager { get; set; }
        public LongestStoredVehicleDto? MostDaysStoredVehicle { get; set; }
        
        public string BestYear { get; set; } = string.Empty;
        public string SlowestYear { get; set; } = string.Empty;
        public string BestMonth { get; set; } = string.Empty;
        public string SlowestMonth { get; set; } = string.Empty;
        public bool IsPropertyHidden { get; set; }
    }

    public class MonthlyBookingDataDto
    {
        public string Month { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
    }

    public class YearlyBookingDataDto
    {
        public string Year { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
    }

    public class BestManagerDto
    {
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int PickupsDone { get; set; }
        public int WeeklySubmissionsDone { get; set; }
        public int OnDemandSubmissionsDone { get; set; }
        public int TotalScore { get; set; }
    }

    public class LongestStoredVehicleDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DaysStored { get; set; }
    }

    public class GetLotOwnerDashboardMetricsQueryHandler : IRequestHandler<GetLotOwnerDashboardMetricsQuery, BaseResponse<LotOwnerDashboardMetricsDto>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Payment> _paymentRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<MaintenanceTask> _taskRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public GetLotOwnerDashboardMetricsQueryHandler(
            IGenericRepository<Booking> bookingRepo, 
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.Payment> paymentRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<MaintenanceTask> taskRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _paymentRepo = paymentRepo;
            _lotManagerRepo = lotManagerRepo;
            _pickupRepo = pickupRepo;
            _taskRepo = taskRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<LotOwnerDashboardMetricsDto>> Handle(GetLotOwnerDashboardMetricsQuery request, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepo.FindAsync(p => p.LotOwnerId == request.LotOwnerId);
            var isHidden = properties.Any(p => p.IsHidden);

            var bookings = await _bookingRepo.FindAsync(b => b.Property.LotOwnerId == request.LotOwnerId, "Property", "Vehicle", "Vehicle.Owner", "Vehicle.Images");
            var payments = await _paymentRepo.FindAsync(p => p.Booking.Property.LotOwnerId == request.LotOwnerId && p.Status == "paid");

            var dto = new LotOwnerDashboardMetricsDto();
            dto.IsPropertyHidden = isHidden;

            if (!bookings.Any())
            {
                return BaseResponse<LotOwnerDashboardMetricsDto>.Ok(dto);
            }

            dto.TotalBookings = bookings.Count();
            dto.TotalRevenue = payments.Sum(p => p.PropertyOwnerAmount + p.PickupChargeAmount);
            dto.NetProfit = dto.TotalRevenue; // Same as Total Revenue unless deductions apply

            var uniqueUsers = bookings.Select(b => b.OwnerId).Distinct().Count();
            var repeatingUsers = bookings.GroupBy(b => b.OwnerId).Count(g => g.Count() > 1);
            dto.CustomerRetentionRate = uniqueUsers > 0 ? Math.Round((double)repeatingUsers / uniqueUsers * 100, 1) : 0;

            var last12Months = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-i)).OrderBy(d => d).ToList();
            var monthlyData = new List<MonthlyBookingDataDto>();
            foreach (var month in last12Months)
            {
                var count = bookings.Count(b => b.StartDate.Year == month.Year && b.StartDate.Month == month.Month);
                monthlyData.Add(new MonthlyBookingDataDto
                {
                    Month = month.ToString("MMM"),
                    BookingsCount = count
                });
            }
            dto.MonthlyBookings = monthlyData;

            var last5Years = Enumerable.Range(0, 5).Select(i => DateTime.UtcNow.Year - i).OrderBy(y => y).ToList();
            var yearlyDataGraph = new List<YearlyBookingDataDto>();
            foreach (var year in last5Years)
            {
                var count = bookings.Count(b => b.StartDate.Year == year);
                yearlyDataGraph.Add(new YearlyBookingDataDto
                {
                    Year = year.ToString(),
                    BookingsCount = count
                });
            }
            dto.YearlyBookings = yearlyDataGraph;

            var lotManagersForOwner = await _lotManagerRepo.FindAsync(lm => lm.Property.LotOwnerId == request.LotOwnerId, "Manager");
            var managerStats = new List<BestManagerDto>();

            foreach(var lm in lotManagersForOwner)
            {
                var pickups = await _pickupRepo.FindAsync(p => p.ManagerId == lm.Id && (int)p.Status >= 6);
                var tasks = await _taskRepo.FindAsync(t => t.ManagerId == lm.Id && t.Status == GD1.Domain.Entities.Enums.MaintenanceTaskStatus.Completed);
                
                int pickupsDone = pickups.Count();
                int weeklyDone = tasks.Count(t => t.Type == GD1.Domain.Entities.Enums.MaintenanceTaskType.WeeklyConditionCheck);
                int onDemandDone = tasks.Count(t => t.Type == GD1.Domain.Entities.Enums.MaintenanceTaskType.OnDemandImage);

                int totalScore = pickupsDone + weeklyDone + onDemandDone;

                managerStats.Add(new BestManagerDto {
                    UserId = lm.ManagerId,
                    Name = lm.Manager?.FullName ?? "",
                    AvatarUrl = !string.IsNullOrEmpty(lm.SelfieUrl) ? lm.SelfieUrl : (lm.Manager?.AvatarUrl ?? ""),
                    PickupsDone = pickupsDone,
                    WeeklySubmissionsDone = weeklyDone,
                    OnDemandSubmissionsDone = onDemandDone,
                    TotalScore = totalScore
                });
            }

            dto.BestPerformedManager = managerStats.OrderByDescending(m => m.TotalScore).FirstOrDefault();

            var longestBooking = bookings.OrderByDescending(b => 
                (b.EndDate < DateTime.UtcNow ? b.EndDate : DateTime.UtcNow) - b.StartDate).FirstOrDefault();

            if (longestBooking != null && longestBooking.Vehicle != null)
            {
                var vehicleImage = longestBooking.Vehicle.Images?.FirstOrDefault()?.ImageUrl ?? "";

                dto.MostDaysStoredVehicle = new LongestStoredVehicleDto
                {
                    Brand = longestBooking.Vehicle.Brand,
                    Model = longestBooking.Vehicle.Model,
                    RegistrationNo = longestBooking.Vehicle.RegistrationNo,
                    ImageUrl = vehicleImage,
                    DaysStored = (int)((longestBooking.EndDate < DateTime.UtcNow ? longestBooking.EndDate : DateTime.UtcNow) - longestBooking.StartDate).TotalDays
                };
            }

            var yearlyData = bookings.GroupBy(b => b.StartDate.Year)
                                     .Select(g => new { Year = g.Key, Count = g.Count() })
                                     .OrderByDescending(x => x.Count).ToList();

            if (yearlyData.Any())
            {
                dto.BestYear = yearlyData.First().Year.ToString();
                dto.SlowestYear = yearlyData.Last().Year.ToString();
            }

            var monthlyTotalData = bookings.GroupBy(b => new { b.StartDate.Year, b.StartDate.Month })
                                           .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Count = g.Count() })
                                           .OrderByDescending(x => x.Count).ToList();

            if (monthlyTotalData.Any())
            {
                dto.BestMonth = monthlyTotalData.First().Date.ToString("MMMM yyyy");
                dto.SlowestMonth = monthlyTotalData.Last().Date.ToString("MMMM yyyy");
            }

            return BaseResponse<LotOwnerDashboardMetricsDto>.Ok(dto);
        }
    }
}
