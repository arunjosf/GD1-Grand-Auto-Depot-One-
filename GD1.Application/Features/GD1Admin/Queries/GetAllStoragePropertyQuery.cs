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
        
        // New search parameters
        public string? Name { get; set; }
        public string? ExtraFacilities { get; set; }
        public bool? HasCCTV { get; set; }
        public bool? HasSecurity { get; set; }
        public bool? HasFireSafety { get; set; }
        
        public bool Recommend { get; set; }
        public GD1.Domain.Entities.Enums.UserRole UserRole { get; set; }
    }

    public class GetAllStoragePropertyQueryHandler : IRequestHandler<GetAllStoragePropertyQuery, BaseResponse<IEnumerable<StoragePropertyListDto>>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;

        public GetAllStoragePropertyQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo)
        {
            _propertyRepo = propertyRepo;
            _vehicleRepo = vehicleRepo;
        }

        public async Task<BaseResponse<IEnumerable<StoragePropertyListDto>>> Handle(GetAllStoragePropertyQuery query, CancellationToken ct)
        {
            bool isAdmin = query.UserRole == GD1.Domain.Entities.Enums.UserRole.GD1Admin || query.UserRole == GD1.Domain.Entities.Enums.UserRole.Manager;

            if (!isAdmin)
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
            }

            var properties = await _propertyRepo.FindAsync(p => 
                p.Status == "Active" &&
                (string.IsNullOrEmpty(query.City) || p.City.ToLower() == query.City.ToLower()) && 
                (string.IsNullOrEmpty(query.Name) || p.Name.Contains(query.Name)) &&
                (string.IsNullOrEmpty(query.ExtraFacilities) || (p.ExtraFacilities != null && p.ExtraFacilities.Contains(query.ExtraFacilities))) &&
                (!query.HasCCTV.HasValue || p.HasCCTV == query.HasCCTV.Value) &&
                (!query.HasSecurity.HasValue || p.HasSecurity == query.HasSecurity.Value) &&
                (!query.HasFireSafety.HasValue || p.HasFireSafety == query.HasFireSafety.Value), "Slots", "LotOwner");

            var resultDtos = new List<StoragePropertyListDto>();

            foreach (var prop in properties)
            {
                var availableSlots = prop.Slots.Where(s => !s.IsOccupied);
                
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
                        Slots = prop.Slots.Select(s => new LotSlotDto
                        {
                            Id = s.Id,
                            SlotNumber = s.SlotNumber,
                            IsOccupied = s.IsOccupied,
                            ImageUrl = s.ImageUrl,
                            SquareFeet = s.SquareFeet,
                            HeightFeet = s.HeightFeet,
                            IsCompatible = vehicle == null || (s.SquareFeet >= (vehicle.LengthFeet * vehicle.WidthFeet) && s.HeightFeet >= vehicle.HeightFeet)
                        }).ToList()
                    };

                    resultDtos.Add(dto);
                }
            }

            // If recommending, sort by distance or price etc. Here we sort by distance.
            if (query.Recommend)
            {
                resultDtos = resultDtos.OrderBy(d => d.DistanceKm).ToList();
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
