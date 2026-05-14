using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllStoragePropertyQuery
       : IRequest<BaseResponse<IEnumerable<StoragePropertyListDto>>>
    {
        public string? City { get; set; }
        public string? Status { get; set; }
    }


    public class GetAllStorageLotsQueryHandler
        : IRequestHandler<GetAllStoragePropertyQuery,
                          BaseResponse<IEnumerable<StoragePropertyListDto>>>
    {
        private readonly IGenericRepository<StorageLot> _lotRepo;
        private readonly IGenericRepository<LotUnit> _unitRepo;
        private readonly IFranchiseReadRepository _franchiseRead;

        public GetAllStorageLotsQueryHandler(
            IGenericRepository<StorageLot> lotRepo,
            IGenericRepository<LotUnit> unitRepo,
            IFranchiseReadRepository franchiseRead)
        {
            _lotRepo = lotRepo;
            _unitRepo = unitRepo;
            _franchiseRead = franchiseRead;
        }

        public async Task<BaseResponse<IEnumerable<StoragePropertyListDto>>> Handle(
            GetAllStoragePropertyQuery query, CancellationToken ct)
        {
            var allLots = await _lotRepo.GetAllAsync();
            var activeLots = allLots.Where(l => l.Status == "Active");

            if (!string.IsNullOrEmpty(query.City))
                activeLots = activeLots.Where(l => l.City.Equals(query.City, StringComparison.OrdinalIgnoreCase));

            var results = new List<StoragePropertyListDto>();

            foreach (var l in activeLots)
            {
                if (!l.LotUnitId.HasValue) continue;

                var unitEntity = await _unitRepo.GetByIdAsync(l.LotUnitId.Value);
                if (unitEntity == null) continue;

                var app = await _franchiseRead.GetByIdAsync(unitEntity.FranchiseApplicationId, 0);
                
                if (app == null || app.Status != FranchiseStatus.Approved) 
                    continue;

                var unitData = app.LotUnits.FirstOrDefault(u => u.Id == l.LotUnitId.Value);

                var dto = new StoragePropertyListDto
                {
                    Id = l.Id,
                    LotCode = l.LotCode,
                    Name = l.Name,
                    City = l.City,
                    State = l.State,
                    Status = l.Status,
                    Tier = l.Tier,
                    TotalSlots = l.TotalSlots,
                    PricePerDay = l.PricePerDay,
                    AverageRating = l.AverageRating,
                    AddressLine = l.AddressLine,
                    ExtraFacilities = l.ExtraFacilities?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [],
                    
                    HasCCTV = unitEntity.HasCCTV,
                    HasSecurity = unitEntity.HasSecurity,
                    HasWorkshop = unitEntity.HasWorkshop,
                    HasWashingArea = unitEntity.HasWashingArea,
                    HasFireSafety = unitEntity.HasFireSafety,
                    Capacity = unitEntity.Capacity,
                    UnitLabel = unitData?.Label ?? unitEntity.Label,

                    OwnerName = app.OwnerName,
                    OwnerEmail = app.ContactEmail,
                    FrontImageUrl = app.FrontImageUrl,
                    OtherImageUrls = app.OtherImageUrls
                };

                // Hydrate Unit Images
                var unitImages = unitData?.OwnerImages
                    .Where(i => !string.IsNullOrEmpty(i.ImageUrl))
                    .Select(i => i.ImageUrl).ToList() ?? [];

                if (!unitImages.Any())
                {
                    // FAIL-SAFE: Use the direct repository method which uses Dapper internally
                    var fallbackImages = await _franchiseRead.GetUnitImageUrlsAsync(l.LotUnitId.Value);
                    unitImages = fallbackImages.ToList();
                }

                dto.UnitImages = unitImages;
                results.Add(dto);
            }

            return BaseResponse<IEnumerable<StoragePropertyListDto>>.Ok(results);
        }
    }
}
