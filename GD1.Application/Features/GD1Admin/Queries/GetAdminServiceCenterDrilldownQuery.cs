using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class AdminServiceCenterDrilldownDto
    {
        public long CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int TotalJobsCompleted { get; set; }
        public int ActiveJobs { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public List<AdminServiceJobDto> RecentJobs { get; set; } = new();
    }

    public class AdminServiceJobDto
    {
        public long JobId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ServiceCost { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }

    public class GetAdminServiceCenterDrilldownQuery : IRequest<BaseResponse<AdminServiceCenterDrilldownDto>>
    {
        public long CenterId { get; set; }
    }

    public class GetAdminServiceCenterDrilldownQueryHandler : IRequestHandler<GetAdminServiceCenterDrilldownQuery, BaseResponse<AdminServiceCenterDrilldownDto>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _centerRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;

        public GetAdminServiceCenterDrilldownQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> centerRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo)
        {
            _centerRepo = centerRepo;
            _userRepo = userRepo;
            _requestRepo = requestRepo;
        }

        public async Task<BaseResponse<AdminServiceCenterDrilldownDto>> Handle(GetAdminServiceCenterDrilldownQuery request, CancellationToken cancellationToken)
        {
            var center = await _centerRepo.GetByIdAsync(request.CenterId);
            if (center == null) return BaseResponse<AdminServiceCenterDrilldownDto>.Fail("Service center not found.");

            var owner = await _userRepo.GetByIdAsync(center.AdminId);
            var jobs = await _requestRepo.FindAsync(r => r.ServiceCenterId == center.Id);

            var recentJobs = jobs
                .OrderByDescending(j => j.CreatedAt)
                .Take(10)
                .Select(j => new AdminServiceJobDto
                {
                    JobId = j.Id,
                    ServiceType = j.ServiceType,
                    Status = j.Status,
                    ServiceCost = j.ServiceCost,
                    ScheduledDate = j.ScheduledDate
                }).ToList();

            var dto = new AdminServiceCenterDrilldownDto
            {
                CenterId = center.Id,
                CenterName = center.Name,
                OwnerName = owner?.FullName ?? "Unknown",
                TotalJobsCompleted = jobs.Count(j => j.Status == "Completed"),
                ActiveJobs = jobs.Count(j => j.Status != "Completed" && j.Status != "Cancelled"),
                TotalRevenueGenerated = jobs.Where(j => j.Status == "Completed").Sum(j => j.ServiceCost),
                RecentJobs = recentJobs
            };

            return BaseResponse<AdminServiceCenterDrilldownDto>.Ok(dto);
        }
    }
}
