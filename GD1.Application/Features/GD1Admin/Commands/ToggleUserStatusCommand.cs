using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ToggleUserStatusCommand : IRequest<BaseResponse<bool>>
    {
        public long UserId { get; set; }
    }

    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;

        public ToggleUserStatusCommandHandler(IGenericRepository<GD1.Domain.Entities.User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ToggleUserStatusCommand req, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(req.UserId);
            if (user == null) return BaseResponse<bool>.Fail("User not found.");

            if (user.Role == GD1.Domain.Entities.Enums.UserRole.GD1Admin)
                return BaseResponse<bool>.Fail("Admins cannot be blocked.");

            user.IsActive = !user.IsActive;
            await _userRepo.UpdateAsync(user);

            var status = user.IsActive ? "Activated" : "Blocked";
            return BaseResponse<bool>.Ok(user.IsActive, $"User successfully {status}.");
        }
    }
}
