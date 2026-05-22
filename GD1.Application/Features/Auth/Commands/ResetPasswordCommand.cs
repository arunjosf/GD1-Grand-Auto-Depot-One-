using GD1.Application.Common;
using GD1.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace GD1.Application.Features.Auth.Commands
{
    public class ResetPasswordCommand : IRequest<BaseResponse<string>>
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Length(6).WithMessage("OTP must be exactly 6 digits.")
                .Matches("^[0-9]{6}$").WithMessage("OTP must contain digits only.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, BaseResponse<string>>
    {
        private readonly IAuthService _auth;

        public ResetPasswordCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<string>> Handle(ResetPasswordCommand cmd, CancellationToken ct)
        {
            await _auth.ResetPasswordAsync(cmd.Email, cmd.Otp, cmd.NewPassword);
            return BaseResponse<string>.Ok(string.Empty, "Password reset successfully. You can now log in with your new password.");
        }
    }
}
