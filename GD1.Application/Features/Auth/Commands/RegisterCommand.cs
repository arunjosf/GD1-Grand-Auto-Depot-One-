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
    public class RegisterCommand
    {
        public RegisterRequest Request { get; set; } = null!;
    }

    public class RegisterCommandHandler
    {
        private readonly IAuthService _auth;

        public RegisterCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> HandleAsync(RegisterCommand command)
        {
            var result = await _auth.RegisterAsync(command.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Registered successfully.");
        }
    }
}
