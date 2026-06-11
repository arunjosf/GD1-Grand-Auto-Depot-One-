using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetMyOwnersQuery : IRequest<BaseResponse<IEnumerable<OwnerDto>>>
    {
        public long ManagerUserId { get; set; }
    }

    public class OwnerDto
    {
        public long OwnerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class GetMyOwnersQueryHandler : IRequestHandler<GetMyOwnersQuery, BaseResponse<IEnumerable<OwnerDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;

        public GetMyOwnersQueryHandler(
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo)
        {
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<IEnumerable<OwnerDto>>> Handle(GetMyOwnersQuery request, CancellationToken cancellationToken)
        {
            var managerRecords = await _lotManagerRepo.FindAsync(lm => lm.ManagerId == request.ManagerUserId && lm.IsActive);
            var propertyIds = managerRecords.Select(lm => lm.PropertyId).Distinct().ToList();

            var properties = await _propertyRepo.FindAsync(p => propertyIds.Contains(p.Id));
            var ownerIds = properties.Select(p => p.LotOwnerId).Distinct().ToList();

            var owners = await _userRepo.FindAsync(u => ownerIds.Contains(u.Id));

            var dtos = owners.Select(o => new OwnerDto
            {
                OwnerId = o.Id,
                FullName = o.FullName,
                Email = o.Email
            }).ToList();

            return BaseResponse<IEnumerable<OwnerDto>>.Ok(dtos);
        }
    }
}
