using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Queries
{
    public class GetMyManagersQuery : IRequest<BaseResponse<IEnumerable<ManagerDto>>>
    {
        public long PropertyOwnerId { get; set; }
    }

    public class ManagerDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long PropertyId { get; set; }
    }

    public class GetMyManagersQueryHandler : IRequestHandler<GetMyManagersQuery, BaseResponse<IEnumerable<ManagerDto>>>
    {
        private readonly IGenericRepository<LotManager> _managerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public GetMyManagersQueryHandler(
            IGenericRepository<LotManager> managerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _managerRepo = managerRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<IEnumerable<ManagerDto>>> Handle(GetMyManagersQuery query, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepo.FindAsync(p => p.LotOwnerId == query.PropertyOwnerId);
            var myPropIds = properties.Select(l => l.Id).ToList();

            var allManagers = await _managerRepo.FindAsync(m => myPropIds.Contains(m.PropertyId) && m.IsActive, "Manager");
            
            var result = allManagers.Select(m => new ManagerDto
            {
                Id = m.ManagerId,
                FullName = m.Manager?.FullName ?? "Unknown",
                Email = m.Manager?.Email ?? "Unknown",
                PropertyId = m.PropertyId
            });

            return BaseResponse<IEnumerable<ManagerDto>>.Ok(result);
        }
    }
}
