using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllApplicationsQuery : IRequest<BaseResponse<IEnumerable<ApplicationListDto>>>
    {
        public FranchiseStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
    }

    public class GetAllApplicationsQueryHandler : IRequestHandler<GetAllApplicationsQuery, BaseResponse<IEnumerable<ApplicationListDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetAllApplicationsQueryHandler(IFranchiseReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<ApplicationListDto>>> Handle(GetAllApplicationsQuery query, CancellationToken ct)
        {
            var statusStr = query.Status?.ToString();
            var result = await _repo.GetAllApplicationsAsync(statusStr);
            
            var adminDtos = result.Select(app => new ApplicationListDto
            {
                Id = app.Id,
                ApplicationType = app.ApplicationType,
                BusinessName = app.BusinessName,
                OwnerName = app.OwnerName,
                ContactEmail = app.ContactEmail,
                PhoneNumber = app.PhoneNumber,
                City = app.City,
                State = app.State,
                Status = app.Status ?? FranchiseStatus.Pending,
                IsAiVerified = app.IsAiVerified,
                CreatedAt = app.CreatedAt,
                PropertyFrontImageUrl = app.FrontImageUrl,
                SlotCount = app.Slots.Count
            });

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                adminDtos = adminDtos.Where(a => 
                    a.BusinessName.ToLower().Contains(term) || 
                    a.OwnerName.ToLower().Contains(term) || 
                    a.City.ToLower().Contains(term));
            }

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("Date", StringComparison.OrdinalIgnoreCase) || query.SortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    adminDtos = query.Descending 
                        ? adminDtos.OrderByDescending(x => x.CreatedAt) 
                        : adminDtos.OrderBy(x => x.CreatedAt);
                }
            }

            return BaseResponse<IEnumerable<ApplicationListDto>>.Ok(adminDtos);
        }
    }
}
