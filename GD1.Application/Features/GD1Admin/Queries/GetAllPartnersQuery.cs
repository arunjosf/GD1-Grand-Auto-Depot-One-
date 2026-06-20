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
    public class GetAllPartnersQuery : IRequest<BaseResponse<PartnersDto>>
    {
    }

    public class PartnersDto
    {
        public List<PartnerGarageDto> Garages { get; set; } = new();
        public List<PartnerServiceCenterDto> ServiceCenters { get; set; } = new();
    }

    public class PartnerGarageDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalBookings { get; set; }
    }

    public class PartnerServiceCenterDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalBookings { get; set; }
    }

    public class GetAllPartnersQueryHandler : IRequestHandler<GetAllPartnersQuery, BaseResponse<PartnersDto>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _serviceCenterRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _serviceRequestRepo;

        public GetAllPartnersQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> serviceCenterRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> serviceRequestRepo)
        {
            _propertyRepo = propertyRepo;
            _serviceCenterRepo = serviceCenterRepo;
            _bookingRepo = bookingRepo;
            _serviceRequestRepo = serviceRequestRepo;
        }

        public async Task<BaseResponse<PartnersDto>> Handle(GetAllPartnersQuery request, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepo.FindAsync(x => x.Status == "Active", "LotOwner", "ActivePropertyImages");
            var serviceCenters = await _serviceCenterRepo.FindAsync(x => x.IsActive, "Images");
            
            var bookings = (await _bookingRepo.GetAllAsync()).ToList();
            var serviceRequests = (await _serviceRequestRepo.GetAllAsync()).ToList();

            var dto = new PartnersDto();

            foreach (var prop in properties)
            {
                dto.Garages.Add(new PartnerGarageDto
                {
                    Id = prop.Id,
                    Name = prop.Name,
                    AddressLine = prop.AddressLine,
                    City = prop.City,
                    PhoneNumber = prop.LotOwner?.PhoneNumber ?? "N/A",
                    ImageUrl = prop.ActivePropertyImages.OrderByDescending(x => x.Id).FirstOrDefault()?.ImageUrl,
                    TotalBookings = bookings.Count(b => b.PropertyId == prop.Id)
                });
            }

            foreach (var sc in serviceCenters)
            {
                dto.ServiceCenters.Add(new PartnerServiceCenterDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    AddressLine = sc.AddressLine,
                    City = sc.City,
                    PhoneNumber = sc.PhoneNumber,
                    ImageUrl = sc.Images.OrderByDescending(x => x.Id).FirstOrDefault()?.ImageUrl,
                    TotalBookings = serviceRequests.Count(sr => sr.ServiceCenterId == sc.Id)
                });
            }

            return BaseResponse<PartnersDto>.Ok(dto);
        }
    }
}
