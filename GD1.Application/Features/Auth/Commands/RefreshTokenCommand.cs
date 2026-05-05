using GD1.Application.Common;
using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Auth.Commands
{
    public class RefreshTokenCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandHandler
    {
        private readonly IAuthService _auth;

        public RefreshTokenCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> HandleAsync(
            RefreshTokenCommand command)
        {
            var result = await _auth.RefreshTokenAsync(command.RefreshToken);
            return BaseResponse<AuthResponse>.Ok(result, "Token refreshed.");
        }
    }
}
