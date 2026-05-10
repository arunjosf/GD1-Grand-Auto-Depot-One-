using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GD1.Application.Common;
using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace GD1.Application.Features.Auth.Commands
{
    public class VerifyEmailOtpCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public VerifyOtpRequest Request { get; set; } = null!;
    }

    public class VerifyEmailOtpCommandValidator
        : AbstractValidator<VerifyEmailOtpCommand>
    {
        public VerifyEmailOtpCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Otp)
                .NotEmpty().Length(6)
                .Matches("^[0-9]{6}$")
                .WithMessage("OTP must be exactly 6 digits.");
        }
    }

    public class VerifyEmailOtpCommandHandler
        : IRequestHandler<VerifyEmailOtpCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;

        public VerifyEmailOtpCommandHandler(IAuthService auth)
            => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> Handle(
            VerifyEmailOtpCommand cmd, CancellationToken ct)
        {
            var result = await _auth.VerifyEmailOtpAsync(cmd.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Email verified successfully.");
        }
    }
}
