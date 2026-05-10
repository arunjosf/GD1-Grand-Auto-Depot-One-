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
    public class RegisterCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public RegisterRequest Request { get; set; } = null!;
    }

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Request.FullName)
                .NotEmpty().WithMessage("Full name is Required")
                .MinimumLength(3).WithMessage("Please enter your full name")
                .Matches(@"^[A-Za-z\s]+$").WithMessage("Full name must contain only letters");

            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            
            RuleFor(x => x.Request.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required.")
                .Equal(x => x.Request.Password)
                .WithMessage("Passwords do not match. Please ensure password and confirm password are the same.");
                
        }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;

        public RegisterCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var result = await _auth.RegisterAsync(request.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Registered successfully.");
        }
    }
}
