using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace GD1.Application.Features.Auth.Commands
{
    public class AddUserMobileCommand : IRequest<BaseResponse<bool>>
    {
        public long UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class AddUserMobileCommandHandler : IRequestHandler<AddUserMobileCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<User> _userRepo;

        public AddUserMobileCommandHandler(IGenericRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<bool>> Handle(AddUserMobileCommand cmd, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(cmd.UserId);
            if (user == null) return BaseResponse<bool>.Fail("User not found.");

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                return BaseResponse<bool>.Fail("Mobile number is already registered for this account.");

            user.PhoneNumber = cmd.PhoneNumber;
            await _userRepo.UpdateAsync(user);

            return BaseResponse<bool>.Ok(true, "Mobile number added successfully.");
        }
    }
}
