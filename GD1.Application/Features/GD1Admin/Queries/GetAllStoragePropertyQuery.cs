using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllStoragePropertyQuery : IRequest<BaseResponse<IEnumerable<StoragePropertyListDto>>>
    {
        public string? City { get; set; }
        public long? VehicleId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? LotOwnerId { get; set; }
        
        // New search parameters
        public string? Name { get; set; }
        public string? ExtraFacilities { get; set; }
        public bool? HasCCTV { get; set; }
        public bool? HasSecurity { get; set; }
        public bool? HasFireSafety { get; set; }
        
        public bool Recommend { get; set; }
        public GD1.Domain.Entities.Enums.UserRole UserRole { get; set; }
        public long UserId { get; set; }
    }

    public class GetAllStoragePropertyQueryHandler : IRequestHandler<GetAllStoragePropertyQuery, BaseResponse<IEnumerable<StoragePropertyListDto>>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly GD1.Application.Interfaces.IGeminiService _geminiService;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;

        public GetAllStoragePropertyQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            GD1.Application.Interfaces.IGeminiService geminiService,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo)
        {
            _propertyRepo = propertyRepo;
            _vehicleRepo = vehicleRepo;
            _geminiService = geminiService;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<IEnumerable<StoragePropertyListDto>>> Handle(GetAllStoragePropertyQuery query, CancellationToken ct)
        {
            bool isAdmin = query.UserRole == GD1.Domain.Entities.Enums.UserRole.GD1Admin || query.UserRole == GD1.Domain.Entities.Enums.UserRole.Manager || query.UserRole == GD1.Domain.Entities.Enums.UserRole.LotOwner;

            if (!isAdmin && !query.Recommend)
            {
                if (string.IsNullOrWhiteSpace(query.City))
                    return BaseResponse<IEnumerable<StoragePropertyListDto>>.Fail("City is a required field to find partnered lots.");
                
                if (!query.VehicleId.HasValue || query.VehicleId <= 0)
                    return BaseResponse<IEnumerable<StoragePropertyListDto>>.Fail("Vehicle ID is a required field to find partnered lots with compatible dimensions.");
            }

            GD1.Domain.Entities.Vehicle? vehicle = null;
            if (query.VehicleId.HasValue)
            {
                vehicle = await _vehicleRepo.GetByIdAsync(query.VehicleId.Value);
                if (vehicle == null && !isAdmin)
                    return BaseResponse<IEnumerable<StoragePropertyListDto>>.Fail("Vehicle not found.");

                if (vehicle != null && !isAdmin && vehicle.OwnerId != query.UserId)
                    return BaseResponse<IEnumerable<StoragePropertyListDto>>.Fail("You can only search using your own vehicle.");
            }

            var properties = await _propertyRepo.FindAsync(p => 
                p.Status == "Active" &&
                (string.IsNullOrEmpty(query.City) || p.City.ToLower() == query.City.ToLower()) && 
                (string.IsNullOrEmpty(query.Name) || p.Name.Contains(query.Name)) &&
                (string.IsNullOrEmpty(query.ExtraFacilities) || (p.ExtraFacilities != null && p.ExtraFacilities.Contains(query.ExtraFacilities))) &&
                (!query.HasCCTV.HasValue || p.HasCCTV == query.HasCCTV.Value) &&
                (!query.HasSecurity.HasValue || p.HasSecurity == query.HasSecurity.Value) &&
                (!query.HasFireSafety.HasValue || p.HasFireSafety == query.HasFireSafety.Value) &&
                (!query.LotOwnerId.HasValue || p.LotOwnerId == query.LotOwnerId.Value), "Slots", "LotOwner", "ActivePropertyImages", "Reviews");

            // Fallback: If city was specified but no properties found, drop the city filter to suggest nearest alternatives
            if (!string.IsNullOrEmpty(query.City) && !properties.Any())
            {
                properties = await _propertyRepo.FindAsync(p => 
                    p.Status == "Active" &&
                    (string.IsNullOrEmpty(query.Name) || p.Name.Contains(query.Name)) &&
                    (string.IsNullOrEmpty(query.ExtraFacilities) || (p.ExtraFacilities != null && p.ExtraFacilities.Contains(query.ExtraFacilities))) &&
                    (!query.HasCCTV.HasValue || p.HasCCTV == query.HasCCTV.Value) &&
                    (!query.HasSecurity.HasValue || p.HasSecurity == query.HasSecurity.Value) &&
                    (!query.HasFireSafety.HasValue || p.HasFireSafety == query.HasFireSafety.Value) &&
                    (!query.LotOwnerId.HasValue || p.LotOwnerId == query.LotOwnerId.Value), "Slots", "LotOwner", "ActivePropertyImages", "Reviews");
            }

            var resultDtos = new List<StoragePropertyListDto>();

            var activeBookings = await _bookingRepo.FindAsync(b => b.Status != GD1.Domain.Entities.Enums.BookingStatus.Completed && b.Status != GD1.Domain.Entities.Enums.BookingStatus.Cancelled && b.SlotId.HasValue);
            var now = DateTime.UtcNow;

            foreach (var prop in properties)
            {
                var availableSlots = prop.Slots.AsEnumerable().Select(s => 
                {
                    // Dynamically consider a slot unoccupied if its booking has expired
                    if (s.IsOccupied)
                    {
                        var slotBooking = activeBookings.FirstOrDefault(b => b.SlotId == s.Id);
                        if (slotBooking == null || slotBooking.EndDate <= now)
                        {
                            s.IsOccupied = false;
                        }
                    }
                    return s;
                });
                
                bool hasCompatibleSlots = true;

                if (vehicle != null)
                {
                    var vehicleArea = vehicle.LengthFeet * vehicle.WidthFeet;
                    var vehicleHeight = vehicle.HeightFeet;

                    var compSlots = availableSlots.Where(s => 
                        s.SquareFeet >= vehicleArea && 
                        s.HeightFeet >= vehicleHeight);

                    // STRICT FILTER: Only consider compatible slots
                    availableSlots = compSlots;
                    hasCompatibleSlots = compSlots.Any();
                }

                // If Admin, hasCompatibleSlots defaults to true and availableSlots is just unoccupied slots
                // Only show properties that have AT LEAST ONE compatible slot (or if Admin without vehicle filter, at least one unoccupied slot or even empty lot if desired. Actually, show if any available)
                if (hasCompatibleSlots && (availableSlots.Any() || isAdmin))
                {
                    double distanceKm = 0;
                    bool isPickupAvailable = false;
                    
                    if (query.Latitude.HasValue && query.Longitude.HasValue && prop.Latitude.HasValue && prop.Longitude.HasValue)
                    {
                        distanceKm = CalculateDistance(query.Latitude.Value, query.Longitude.Value, prop.Latitude.Value, prop.Longitude.Value);
                        if (distanceKm <= 40) 
                        {
                            isPickupAvailable = true;
                        }
                    }

                    var dto = new StoragePropertyListDto
                    {
                        Id = prop.Id,
                        LotCode = prop.LotCode,
                        Name = prop.Name,
                        City = prop.City,
                        State = prop.State,
                        Status = prop.Status,
                        Tier = "Premium Private Garage",
                        TotalSlots = prop.Slots.Count,
                        AvailableSlots = prop.Slots.Count(s => !s.IsOccupied),
                        PricePerDay = prop.PricePerDay,
                        AverageRating = prop.AverageRating,
                        TotalReviews = prop.TotalReviews,
                        Latitude = prop.Latitude,
                        Longitude = prop.Longitude,
                        DistanceKm = distanceKm,
                        IsPickupAvailable = isPickupAvailable,
                        HasCompatibleSlots = hasCompatibleSlots,
                        ContactInfo = new LotContactDto
                        {
                            AddressLine = prop.AddressLine,
                            OwnerPhone = prop.LotOwner?.PhoneNumber
                        },
                        PropertyDetails = new LotPropertyDetailDto
                        {
                            AddressLine = prop.AddressLine,
                            HasCCTV = prop.HasCCTV,
                            HasSecurity = prop.HasSecurity,
                            HasWorkshop = prop.HasWorkshopBay,
                            HasWashingArea = prop.HasWashingArea,
                            HasFireSafety = prop.HasFireSafety,
                            ExtraFacilities = prop.ExtraFacilities
                        },
                        PropertyImages = prop.ActivePropertyImages != null ? prop.ActivePropertyImages.Select(pi => pi.ImageUrl).ToList() : new List<string>(),
                        Slots = availableSlots.Select(s => new LotSlotDto
                        {
                            Id = s.Id,
                            SlotNumber = s.SlotNumber,
                            IsOccupied = s.IsOccupied,
                            ImageUrl = s.ImageUrl,
                            SquareFeet = s.SquareFeet,
                            HeightFeet = s.HeightFeet,
                            IsCompatible = true
                        }).ToList(),
                        RecentReviews = prop.Reviews.Where(r => !string.IsNullOrWhiteSpace(r.Comment)).Select(r => r.Comment!).Take(5).ToList()
                    };

                    resultDtos.Add(dto);
                }
            }

            // Always sort by distance first
            resultDtos = resultDtos.OrderBy(d => d.DistanceKm).ToList();

            // Always use Gemini API to evaluate amenities and reviews and attach the badge
            if (resultDtos.Any())
            {
                bool aiSucceeded = false;
                try
                {
                    var aiRec = await _geminiService.GetBestLotRecommendationAsync(resultDtos, "Provide the best property considering the amenities and the user reviews.");
                    if (aiRec != null && aiRec.BestLotId > 0)
                    {
                        var bestLot = resultDtos.FirstOrDefault(d => d.Id == aiRec.BestLotId);
                        if (bestLot != null)
                        {
                            bestLot.IsRecommendedByAi = true;
                            bestLot.AiRecommendationReason = aiRec.AiAnalysis;
                            aiSucceeded = true;
                        }
                    }
                }
                catch { /* AI failed, use fallback below */ }

                // Fallback: if AI failed or returned no valid lot, pick highest-rated lot
                if (!aiSucceeded)
                {
                    var bestByRating = resultDtos.OrderByDescending(d => d.AverageRating).ThenByDescending(d => d.TotalReviews).FirstOrDefault();
                    if (bestByRating != null)
                    {
                        bestByRating.IsRecommendedByAi = true;
                        bestByRating.AiRecommendationReason = "Recommended based on top customer ratings and reviews among nearby garages.";
                    }
                }
            }

            return BaseResponse<IEnumerable<StoragePropertyListDto>>.Ok(resultDtos);
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
