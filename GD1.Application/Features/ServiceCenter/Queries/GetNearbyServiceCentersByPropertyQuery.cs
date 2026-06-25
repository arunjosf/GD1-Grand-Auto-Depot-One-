using GD1.Application.Common;
using GD1.Application.Features.ServiceCenter.DTOs;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetNearbyServiceCentersByPropertyQuery : IRequest<BaseResponse<IEnumerable<ServiceCenterDto>>>
    {
        public long PropertyId { get; set; }
    }

    public class GetNearbyServiceCentersByPropertyQueryHandler : IRequestHandler<GetNearbyServiceCentersByPropertyQuery, BaseResponse<IEnumerable<ServiceCenterDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> _imageRepo;

        public GetNearbyServiceCentersByPropertyQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> imageRepo)
        {
            _scRepo = scRepo;
            _propertyRepo = propertyRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<IEnumerable<ServiceCenterDto>>> Handle(
            GetNearbyServiceCentersByPropertyQuery request, CancellationToken cancellationToken)
        {
            var property = await _propertyRepo.GetByIdAsync(request.PropertyId);
            if (property == null || property.Latitude == null || property.Longitude == null)
            {
                return BaseResponse<IEnumerable<ServiceCenterDto>>.Ok(new List<ServiceCenterDto>());
            }

            var allCenters = await _scRepo.GetAllAsync();
            var approvedCenters = allCenters.Where(sc => sc.Status == "Approved");

            var allImages = await _imageRepo.GetAllAsync();

            var nearbyCenters = approvedCenters.Where(sc => 
                sc.Latitude != null && 
                sc.Longitude != null && 
                CalculateDistance(property.Latitude.Value, property.Longitude.Value, sc.Latitude.Value, sc.Longitude.Value) <= 25.0
            ).Select(sc => new ServiceCenterDto
            {
                Id = sc.Id,
                Name = sc.Name,
                PhoneNumber = sc.PhoneNumber,
                Email = sc.Email,
                AddressLine = sc.AddressLine,
                City = sc.City,
                District = sc.District,
                State = sc.State,
                Country = sc.Country,
                PostalCode = sc.PostalCode,
                Latitude = sc.Latitude,
                Longitude = sc.Longitude,
                DistanceKm = CalculateDistance(property.Latitude.Value, property.Longitude.Value, sc.Latitude.Value, sc.Longitude.Value),
                ImageUrl = allImages.FirstOrDefault(i => i.ServiceCenterId == sc.Id)?.ImageUrl
            }).ToList();

            return BaseResponse<IEnumerable<ServiceCenterDto>>.Ok(nearbyCenters);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            var d = R * c; // Distance in km
            return d;
        }

        private double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180d);
        }
    }
}
