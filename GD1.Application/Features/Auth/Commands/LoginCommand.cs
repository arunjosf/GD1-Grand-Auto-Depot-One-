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
    public class LoginCommand
    {
        public LoginRequest Request { get; set; } = null!;
    }

    public class LoginCommandHandler
    {
        private readonly IAuthService _auth;

        public LoginCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> HandleAsync(LoginCommand command)
        {
            var result = await _auth.LoginAsync(command.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Login successful.");
        }
    }
}
