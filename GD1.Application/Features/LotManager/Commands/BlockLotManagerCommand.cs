using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManagement.Commands
{
    public class BlockLotManagerCommand : IRequest<BaseResponse<string>>
    {
        /// <summary>The authenticated LotOwner's user ID.</summary>
        public long LotOwnerId { get; set; }

        /// <summary>The User ID of the manager to block.</summary>
        public long ManagerUserId { get; set; }
    }

    public class BlockLotManagerCommandHandler : IRequestHandler<BlockLotManagerCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;

        public BlockLotManagerCommandHandler(
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo)
        {
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<string>> Handle(BlockLotManagerCommand cmd, CancellationToken ct)
        {
            // 1. Get all properties owned by this LotOwner
            var ownedProperties = await _propertyRepo.FindAsync(p => p.LotOwnerId == cmd.LotOwnerId);
            var ownedPropertyIds = ownedProperties.Select(p => p.Id).ToList();

            if (!ownedPropertyIds.Any())
                return BaseResponse<string>.Fail("You have no registered properties.");

            // 2. Verify the target user is a manager on at least one of the owner's properties
            var managerRecords = await _lotManagerRepo.FindAsync(
                m => m.ManagerId == cmd.ManagerUserId && ownedPropertyIds.Contains(m.PropertyId));

            if (!managerRecords.Any())
                return BaseResponse<string>.Fail("This user is not a manager of any of your properties.");

            // 3. Find the user account
            var managerUser = await _userRepo.GetByIdAsync(cmd.ManagerUserId);
            if (managerUser is null)
                return BaseResponse<string>.Fail("Manager user account not found.");

            // 4. Block only — LotOwners cannot unblock
            if (!managerUser.IsActive)
                return BaseResponse<string>.Fail("This manager is already blocked. Contact GD1 Admin to unblock.");

            managerUser.IsActive = false;
            await _userRepo.UpdateAsync(managerUser);

            return BaseResponse<string>.Ok(string.Empty, $"{managerUser.FullName} has been blocked successfully.");
        }
    }
}
