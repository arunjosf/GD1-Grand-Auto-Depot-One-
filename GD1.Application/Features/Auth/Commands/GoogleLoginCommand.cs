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
    public class GoogleLoginCommand
    {
        public GoogleLoginRequest Request { get; set; } = null!;
    }

    public class GoogleLoginCommandHandler
    {
        private readonly IAuthService _auth;

        public GoogleLoginCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> HandleAsync(
            GoogleLoginCommand command)
        {
            var result = await _auth.GoogleLoginAsync(command.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Google login successful.");
        }
    }
}
