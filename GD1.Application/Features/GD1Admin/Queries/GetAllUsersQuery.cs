using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllUsersQuery : IRequest<BaseResponse<IEnumerable<UserListDto>>>
    {
        public UserRole? Role { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, BaseResponse<IEnumerable<UserListDto>>>
    {
        private readonly IUserReadRepository _repo;

        public GetAllUsersQueryHandler(IUserReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<UserListDto>>> Handle(GetAllUsersQuery req, CancellationToken ct)
        {
            var result = await _repo.GetAllUsersAsync(req.Role, req.SearchTerm);
            return BaseResponse<IEnumerable<UserListDto>>.Ok(result);
        }
    }
}
