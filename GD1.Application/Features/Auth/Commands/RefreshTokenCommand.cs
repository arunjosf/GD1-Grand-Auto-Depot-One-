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
    public class RefreshTokenCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required in the cookie.");
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;

        public RefreshTokenCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await _auth.RefreshTokenAsync(request.RefreshToken);
            return BaseResponse<AuthResponse>.Ok(result, "Token refreshed.");
        }
    }
}
