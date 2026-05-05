using GD1.Application.Common;
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
    public class LogoutCommand : IRequest<BaseResponse<string>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required in the cookie.");
        }
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, BaseResponse<string>>
    {
        private readonly IAuthService _auth;

        public LogoutCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await _auth.RevokeTokenAsync(request.RefreshToken);
            return BaseResponse<string>.Ok(string.Empty, "Logged out successfully.");
        }
    }
}
