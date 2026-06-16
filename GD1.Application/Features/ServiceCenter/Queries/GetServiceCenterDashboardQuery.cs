using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GD1.Application.Features.ServiceRequest.Queries;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetServiceCenterDashboardQuery : IRequest<BaseResponse<ServiceCenterDashboardDto>>
    {
        public long AdminId { get; set; }
    }

    public class ServiceCenterDashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalMechanics { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<MyServiceRequestDto> PendingBookings { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class GetServiceCenterDashboardQueryHandler : IRequestHandler<GetServiceCenterDashboardQuery, BaseResponse<ServiceCenterDashboardDto>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<Mechanics> _mechanicsRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public GetServiceCenterDashboardQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<Mechanics> mechanicsRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<Booking> bookingRepo)
        {
            _scRepo = scRepo;
            _requestRepo = requestRepo;
            _mechanicsRepo = mechanicsRepo;
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<ServiceCenterDashboardDto>> Handle(GetServiceCenterDashboardQuery request, CancellationToken cancellationToken)
        {
            var centers = await _scRepo.FindAsync(x => x.AdminId == request.AdminId, "Images");
            var sc = centers.FirstOrDefault();
            if (sc == null) return BaseResponse<ServiceCenterDashboardDto>.Fail("Service center not found");

            var allRequests = await _requestRepo.FindAsync(x => x.ServiceCenterId == sc.Id);
            var mechanics = await _mechanicsRepo.FindAsync(x => x.ServiceCenterId == sc.Id && !x.IsDeleted);

            var totalRevenue = allRequests.Where(x => x.IsCompleted == true).Sum(x => x.CenterEarning);
            var totalBookings = allRequests.Count();
            var totalMechanics = mechanics.Count();

            // Monthly Revenue for last 6 months
            var monthlyRevenue = new List<MonthlyRevenueDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var revenue = allRequests.Where(x => x.IsCompleted == true && x.CreatedAt.Month == targetMonth.Month && x.CreatedAt.Year == targetMonth.Year).Sum(x => x.CenterEarning);
                monthlyRevenue.Add(new MonthlyRevenueDto { Month = targetMonth.ToString("MMM"), Revenue = revenue });
            }

            var pendingRequests = allRequests.Where(x => x.Status == "Requested" || x.Status == "Pending").OrderByDescending(x => x.CreatedAt).Take(5).ToList();
            var pendingDtos = new List<MyServiceRequestDto>();

            foreach(var pr in pendingRequests)
            {
                var bk = await _bookingRepo.GetByIdAsync(pr.BookingId);
                var vhList = bk != null ? await _vehicleRepo.FindAsync(v => v.Id == bk.VehicleId, "Images") : null;
                var vh = vhList?.FirstOrDefault();

                pendingDtos.Add(new MyServiceRequestDto
                {
                    Id = pr.Id,
                    BookingId = pr.BookingId,
                    VehicleId = vh?.Id ?? 0,
                    VehicleBrand = vh?.Brand ?? "",
                    VehicleModel = vh?.Model ?? "",
                    VehicleRegistrationNo = vh?.RegistrationNo ?? "",
                    ServiceType = pr.ServiceType,
                    Notes = pr.Notes,
                    ScheduledDate = pr.ScheduledDate,
                    Status = pr.Status,
                    CreatedAt = pr.CreatedAt,
                    PropertyCity = pr.Booking?.Property?.City,
                      PropertyAddress = pr.Booking?.Property?.AddressLine,
                      PropertyLatitude = pr.Booking?.Property?.Latitude,
                      PropertyLongitude = pr.Booking?.Property?.Longitude,
                      ServiceCenterLatitude = pr.ServiceCenter?.Latitude,
                      ServiceCenterLongitude = pr.ServiceCenter?.Longitude,
                    ServiceCenterImage = vh?.Images?.FirstOrDefault()?.ImageUrl // Using this field temporarily to pass vehicle image to frontend
                });
            }

            var dto = new ServiceCenterDashboardDto
            {
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                TotalMechanics = totalMechanics,
                MonthlyRevenue = monthlyRevenue,
                PendingBookings = pendingDtos
            };

            return BaseResponse<ServiceCenterDashboardDto>.Ok(dto, "Success");
        }
    }
}
