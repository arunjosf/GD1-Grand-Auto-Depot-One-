using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetNearbyServiceCentersQuery : IRequest<BaseResponse<NearbyServiceCentersResponse>>
    {
        public long VehicleId { get; set; }
        public string? SearchText { get; set; }
    }

    public class NearbyServiceCentersResponse
    {
        public bool HasManagerRecommendation { get; set; }
        public string? ManagerServiceRemarks { get; set; }
        public AiServiceCenterRecommendationResponse? AiRecommendation { get; set; }
        public List<NearbyServiceCenterDto> ServiceCenters { get; set; } = new();
    }

    public class NearbyServiceCenterDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? SupportedBrands { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double DistanceInKm { get; set; }
    }

    public class GetNearbyServiceCentersQueryHandler : IRequestHandler<GetNearbyServiceCentersQuery, BaseResponse<NearbyServiceCentersResponse>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGeminiService _geminiService;

        public GetNearbyServiceCentersQueryHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGeminiService geminiService)
        {
            _vehicleRepo = vehicleRepo;
            _scRepo = scRepo;
            _geminiService = geminiService;
        }

        public async Task<BaseResponse<NearbyServiceCentersResponse>> Handle(GetNearbyServiceCentersQuery request, CancellationToken ct)
        {
            // 1. Get the vehicle and its active booking property location
            var vehicles = await _vehicleRepo.FindAsync(v => v.Id == request.VehicleId, "Bookings.Property");
            var vehicle = vehicles.FirstOrDefault();

            if (vehicle == null)
                return BaseResponse<NearbyServiceCentersResponse>.Fail("Vehicle not found.");

            var activeBooking = vehicle.Bookings.OrderByDescending(b => b.Id).FirstOrDefault(b => b.Status == BookingStatus.InLot);
            
            // Allow if no active booking but fallback to all? We must need a lot's city.
            if (activeBooking == null || activeBooking.Property == null)
                return BaseResponse<NearbyServiceCentersResponse>.Fail("Vehicle is not currently stored in any lot. Location unknown.");

            var lotLat = activeBooking.Property.Latitude ?? 0;
            var lotLon = activeBooking.Property.Longitude ?? 0;
            var lotCity = activeBooking.Property.City;

            // 2. Fetch all approved Service Centers
            var allSc = await _scRepo.GetAllAsync();
            var query = allSc.Where(sc => sc.Status == "Approved");

            // Filter by City
            query = query.Where(sc => sc.City.Equals(lotCity, StringComparison.OrdinalIgnoreCase));

            // Manual Search Text filter
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                var lowerSearch = request.SearchText.ToLower();
                query = query.Where(sc => sc.Name.ToLower().Contains(lowerSearch) || 
                                         (sc.SupportedBrands != null && sc.SupportedBrands.ToLower().Contains(lowerSearch)));
            }

            var results = new List<NearbyServiceCenterDto>();
            foreach (var sc in query)
            {
                var scLat = sc.Latitude ?? 0;
                var scLon = sc.Longitude ?? 0;
                var dist = CalculateDistance(lotLat, lotLon, scLat, scLon);

                results.Add(new NearbyServiceCenterDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    PhoneNumber = sc.PhoneNumber,
                    AddressLine = sc.AddressLine,
                    City = sc.City,
                    SupportedBrands = sc.SupportedBrands,
                    Latitude = sc.Latitude,
                    Longitude = sc.Longitude,
                    DistanceInKm = Math.Round(dist, 2)
                });
            }

            // Order nearest first
            results = results.OrderBy(x => x.DistanceInKm).ToList();

            // 3. AI Recommendation
            AiServiceCenterRecommendationResponse? aiRec = null;
            if (results.Any())
            {
                var top5 = results.Take(5).ToList();
                var serializedStr = string.Join("\n", top5.Select(s => 
                    $"- ID: {s.Id}, Name: {s.Name}, Distance: {s.DistanceInKm}km, Brands: {s.SupportedBrands}"));

                aiRec = await _geminiService.GetBestServiceCenterRecommendationAsync(serializedStr);
            }

            var response = new NearbyServiceCentersResponse
            {
                HasManagerRecommendation = vehicle.HasServiceRecommendation,
                ManagerServiceRemarks = vehicle.ManagerServiceRemarks,
                AiRecommendation = aiRec,
                ServiceCenters = results
            };

            return BaseResponse<NearbyServiceCentersResponse>.Ok(response);
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            if (lat1 == 0 || lon1 == 0 || lat2 == 0 || lon2 == 0) return 9999; // unknown distance
            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = lon1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = lon2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6371 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
