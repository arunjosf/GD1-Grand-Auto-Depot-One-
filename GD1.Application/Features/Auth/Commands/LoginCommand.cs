using GD1.Application.Common;
using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public LoginRequest Request { get; set; } = null!;
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Password).NotEmpty();
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;

        public LoginCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _auth.LoginAsync(request.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Login successful.");
        }
    }
}
