using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllApplicationsQuery
       : IRequest<BaseResponse<IEnumerable<ApplicationListDto>>>
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; } 
        public string? SortBy { get; set; } = "CreatedAt";
        public bool Descending { get; set; } = true;
    }

    public class GetAllApplicationsQueryHandler
        : IRequestHandler<GetAllApplicationsQuery,
                          BaseResponse<IEnumerable<ApplicationListDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetAllApplicationsQueryHandler(IFranchiseReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ApplicationListDto>>> Handle(
            GetAllApplicationsQuery query, CancellationToken ct)
        {
            var result = await _repo.GetAllApplicationsAsync(
                query.Status, query.SearchTerm, query.SortBy, query.Descending);
            return BaseResponse<IEnumerable<ApplicationListDto>>.Ok(result);
        }
    }
}
