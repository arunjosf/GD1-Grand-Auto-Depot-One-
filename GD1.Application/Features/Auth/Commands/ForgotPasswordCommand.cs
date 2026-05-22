using GD1.Application.Common;
using GD1.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace GD1.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommand : IRequest<BaseResponse<string>>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, BaseResponse<string>>
    {
        private readonly IAuthService _auth;

        public ForgotPasswordCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<string>> Handle(ForgotPasswordCommand cmd, CancellationToken ct)
        {
            var message = await _auth.SendPasswordResetOtpAsync(cmd.Email);
            return BaseResponse<string>.Ok(string.Empty, message);
        }
    }
}
