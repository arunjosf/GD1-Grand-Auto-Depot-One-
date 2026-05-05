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
    public class GoogleLoginCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public GoogleLoginRequest Request { get; set; } = null!;
    }

    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.Request.IdToken).NotEmpty();
        }
    }

    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;

        public GoogleLoginCommandHandler(IAuthService auth) => _auth = auth;

        public async Task<BaseResponse<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _auth.GoogleLoginAsync(request.Request);
            return BaseResponse<AuthResponse>.Ok(result, "Google login successful.");
        }
    }
}
