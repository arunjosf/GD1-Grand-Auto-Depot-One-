using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManagement.Commands
{
    public class ToggleBlockLotManagerCommand : IRequest<BaseResponse<bool>>
    {
        public long LotOwnerId { get; set; }
        public long LotManagerRecordId { get; set; }
    }

    public class ToggleBlockLotManagerCommandHandler : IRequestHandler<ToggleBlockLotManagerCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public ToggleBlockLotManagerCommandHandler(
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleBlockLotManagerCommand cmd, CancellationToken ct)
        {
            var lotManager = await _lotManagerRepo.GetByIdAsync(cmd.LotManagerRecordId);
            if (lotManager is null)
                return BaseResponse<bool>.Fail("Manager association not found.");

            var property = await _propertyRepo.GetByIdAsync(lotManager.PropertyId);
            if (property is null || property.LotOwnerId != cmd.LotOwnerId)
                return BaseResponse<bool>.Fail("You do not have permission to manage this manager.");

            lotManager.IsActive = !lotManager.IsActive;
            await _lotManagerRepo.UpdateAsync(lotManager);

            string status = lotManager.IsActive ? "unblocked" : "blocked";
            return BaseResponse<bool>.Ok(lotManager.IsActive, $"Manager has been successfully {status}.");
        }
    }
}
