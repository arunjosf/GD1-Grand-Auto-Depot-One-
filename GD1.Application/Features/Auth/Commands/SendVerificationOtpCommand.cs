using GD1.Application.Common;
using GD1.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace GD1.Application.Features.Auth.Commands
{
    public class SendVerificationOtpCommand : IRequest<BaseResponse<string>>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class SendVerificationOtpCommandValidator
        : AbstractValidator<SendVerificationOtpCommand>
    {
        public SendVerificationOtpCommandValidator()    
        {
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress()
                .WithMessage("Valid email is required.");
        }
    }

    public class SendVerificationOtpCommandHandler
        : IRequestHandler<SendVerificationOtpCommand, BaseResponse<string>>
    {
        private readonly IAuthService _auth;

        public SendVerificationOtpCommandHandler(IAuthService auth)
            => _auth = auth;

        public async Task<BaseResponse<string>> Handle(
            SendVerificationOtpCommand cmd, CancellationToken ct)
        {
            var message = await _auth.SendVerificationOtpAsync(cmd.Email);
            return BaseResponse<string>.Ok(string.Empty, message);
        }
    }
}
