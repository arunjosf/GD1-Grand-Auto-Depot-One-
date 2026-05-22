using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManagement.Commands
{
    public class RemoveLotManagerCommand : IRequest<BaseResponse<bool>>
    {
        /// <summary>The authenticated LotOwner's user ID (injected from the controller).</summary>
        public long LotOwnerId { get; set; }

        /// <summary>The LotManager record ID to deactivate.</summary>
        public long LotManagerRecordId { get; set; }
    }

    public class RemoveLotManagerCommandHandler : IRequestHandler<RemoveLotManagerCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;

        public RemoveLotManagerCommandHandler(
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo)
        {
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<bool>> Handle(RemoveLotManagerCommand cmd, CancellationToken ct)
        {
            // 1. Find the lot manager record
            var lotManager = await _lotManagerRepo.GetByIdAsync(cmd.LotManagerRecordId);
            if (lotManager is null)
                return BaseResponse<bool>.Fail("Manager record not found.");

            // 2. Ensure the property belongs to the calling owner
            var property = await _propertyRepo.GetByIdAsync(lotManager.PropertyId);
            if (property is null || property.LotOwnerId != cmd.LotOwnerId)
                return BaseResponse<bool>.Fail("You do not have permission to remove this manager.");

            if (!lotManager.IsActive)
                return BaseResponse<bool>.Fail("This manager is already inactive.");

            // 3. Deactivate the record
            lotManager.IsActive = false;
            await _lotManagerRepo.UpdateAsync(lotManager);

            // 4. If the manager has no other active assignments on any property,
            //    revert their role back to VehicleOwner
            var otherActiveSlots = await _lotManagerRepo.FindAsync(
                m => m.ManagerId == lotManager.ManagerId && m.IsActive && m.Id != lotManager.Id);

            if (!otherActiveSlots.Any())
            {
                var user = await _userRepo.GetByIdAsync(lotManager.ManagerId);
                if (user != null && user.Role == UserRole.Manager)
                {
                    user.Role = UserRole.VehicleOwner;
                    await _userRepo.UpdateAsync(user);
                }
            }

            return BaseResponse<bool>.Ok(true, "Manager removed from property successfully.");
        }
    }
}
