using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetPartnerServiceCenterDetailQuery : IRequest<BaseResponse<PartnerServiceCenterDetailDto>>
    {
        public long Id { get; set; }
    }

    public class PartnerServiceCenterDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalBookings { get; set; }
        public bool IsHidden { get; set; }
        public bool IsBlocked { get; set; }
        public List<AdminActiveServiceDto> ActiveServices { get; set; } = new();

        public string? OwnerIdProofUrl { get; set; }
    }

    public class AdminActiveServiceDto
    {
        public long Id { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
    }

    public class GetPartnerServiceCenterDetailQueryHandler : IRequestHandler<GetPartnerServiceCenterDetailQuery, BaseResponse<PartnerServiceCenterDetailDto>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _serviceCenterRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;

        public GetPartnerServiceCenterDetailQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> serviceCenterRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo)
        {
            _serviceCenterRepo = serviceCenterRepo;
            _serviceRequestRepo = serviceRequestRepo;
        }

        public async Task<BaseResponse<PartnerServiceCenterDetailDto>> Handle(GetPartnerServiceCenterDetailQuery request, CancellationToken cancellationToken)
        {
            var serviceCenter = (await _serviceCenterRepo.FindAsync(x => x.Id == request.Id, "Images")).FirstOrDefault();
            if (serviceCenter == null) return BaseResponse<PartnerServiceCenterDetailDto>.Fail("Service Center not found.");

            var serviceRequests = await _serviceRequestRepo.FindAsync(x => x.ServiceCenterId == request.Id, "Booking.Vehicle");

            var dto = new PartnerServiceCenterDetailDto
            {
                Id = serviceCenter.Id,
                Name = serviceCenter.Name,
                AddressLine = serviceCenter.AddressLine,
                City = serviceCenter.City,
                State = serviceCenter.State,
                PhoneNumber = serviceCenter.PhoneNumber,
                ImageUrl = serviceCenter.Images?.OrderByDescending(x => x.Id).FirstOrDefault()?.ImageUrl,
                TotalBookings = serviceRequests.Count(),
                IsHidden = serviceCenter.IsHidden,
                IsBlocked = serviceCenter.IsBlocked,
                ActiveServices = serviceRequests
                    .Where(sr => sr.IsCompleted != true && sr.Status != "Completed" && sr.Status != "Cancelled")
                    .Select(sr => new AdminActiveServiceDto
                    {
                        Id = sr.Id,
                        StartDate = sr.CreatedAt.ToString("MMM dd, yyyy"), // Service request creation date
                        EndDate = sr.ScheduledDate?.ToString("MMM dd, yyyy") ?? "N/A",
                        Status = sr.Status.ToString(),
                        VehicleRegistration = sr.Booking?.Vehicle?.RegistrationNo ?? "",
                        VehicleName = $"{sr.Booking?.Vehicle?.Brand} {sr.Booking?.Vehicle?.Model}"
                    }).ToList(),
                OwnerIdProofUrl = serviceCenter.OwnerIdProofUrl
            };

            return BaseResponse<PartnerServiceCenterDetailDto>.Ok(dto);
        }
    }
}
