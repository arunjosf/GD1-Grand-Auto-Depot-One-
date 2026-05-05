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
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Request.FullName).NotEmpty();
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
