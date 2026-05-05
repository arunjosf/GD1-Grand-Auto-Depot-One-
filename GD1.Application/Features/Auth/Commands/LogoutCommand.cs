using GD1.Application.Common;
using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Auth.Commands
{
    public class LogoutCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutCommandHandler
    {
        private readonly IAuthService _auth;

        public LogoutCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<string>> HandleAsync(LogoutCommand command)
        {
            await _auth.RevokeTokenAsync(command.RefreshToken);
            return BaseResponse<string>.Ok(string.Empty, "Logged out successfully.");
        }
    }
}
