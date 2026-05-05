using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class BlockUserCommand : IRequest<BaseResponse<string>>
    {
        public long UserId { get; set; }
    }

    public class BlockUserCommandHandler
        : IRequestHandler<BlockUserCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<User> _repo;

        public BlockUserCommandHandler(IGenericRepository<User> repo)
            => _repo = repo;

        public async Task<BaseResponse<string>> Handle(
            BlockUserCommand cmd, CancellationToken ct)
        {
            var user = await _repo.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new KeyNotFoundException("User not found.");

            // GD1Admin cannot block themselves or other GD1Admins here if needed, but leaving general for now.
            if (user.Role == GD1.Domain.Entities.Enums.UserRole.Admin)
                throw new InvalidOperationException("Cannot block another GD1 Admin.");

            user.IsActive = !user.IsActive;
            await _repo.UpdateAsync(user);

            var msg = user.IsActive ? "User activated." : "User blocked.";
            return BaseResponse<string>.Ok(string.Empty, msg);
        }
    }
}
