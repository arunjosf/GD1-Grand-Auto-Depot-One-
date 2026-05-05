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
        public long LotOwnerId { get; set; }
    }

    public class ManagerDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long LotId { get; set; }
    }

    public class GetMyManagersQueryHandler : IRequestHandler<GetMyManagersQuery, BaseResponse<IEnumerable<ManagerDto>>>
    {
        private readonly IGenericRepository<LotManager> _managerRepo;
        private readonly IGenericRepository<StorageLot> _lotRepo;

        public GetMyManagersQueryHandler(
            IGenericRepository<LotManager> managerRepo,
            IGenericRepository<StorageLot> lotRepo)
        {
            _managerRepo = managerRepo;
            _lotRepo = lotRepo;
        }

        public async Task<BaseResponse<IEnumerable<ManagerDto>>> Handle(GetMyManagersQuery query, CancellationToken cancellationToken)
        {
            var lots = await _lotRepo.GetAllAsync();
            var myLots = lots.Where(l => l.LotOwnerId == query.LotOwnerId).Select(l => l.Id).ToList();

            var allManagers = await _managerRepo.GetAllAsync();
            // Since we need User details, we would ideally use Include(m => m.Manager) but generic repo might not support it directly.
            // Assuming we just map what we have or I should use DbContext for complex queries.
            
            var result = allManagers
                .Where(m => myLots.Contains(m.LotId) && m.IsActive)
                .Select(m => new ManagerDto
                {
                    Id = m.ManagerId,
                    FullName = m.Manager?.FullName ?? "Unknown", // Assuming lazy loading or eager loading is configured
                    Email = m.Manager?.Email ?? "Unknown",
                    LotId = m.LotId
                });

            return BaseResponse<IEnumerable<ManagerDto>>.Ok(result);
        }
    }
}
