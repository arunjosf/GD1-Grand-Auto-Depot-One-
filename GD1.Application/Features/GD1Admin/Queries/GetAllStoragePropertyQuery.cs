using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.GD1Admin.DTOs;

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
        private readonly GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.StorageLot> _repo;

        public GetAllStorageLotsQueryHandler(GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.StorageLot> repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<StoragePropertyListDto>>> Handle(
            GetAllStoragePropertyQuery query, CancellationToken ct)
        {
            var allLots = await _repo.GetAllAsync();

            if (!string.IsNullOrEmpty(query.City))
                allLots = allLots.Where(l => l.City.Equals(query.City, StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(query.Status))
                allLots = allLots.Where(l => l.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));

            var result = allLots.Select(l => new StoragePropertyListDto
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
                OwnerName = l.LotOwner?.FullName ?? "Unknown",
                OwnerEmail = l.LotOwner?.Email ?? "Unknown"
            }).ToList();

            return BaseResponse<IEnumerable<StoragePropertyListDto>>.Ok(result);
        }
    }
}
