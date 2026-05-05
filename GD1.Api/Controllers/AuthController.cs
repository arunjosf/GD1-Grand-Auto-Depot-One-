using GD1.Application.Features.Auth.Commands;
using GD1.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        private void ClearTokenCookies()
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _mediator.Send(new RegisterCommand { Request = req });
            if (result.Success && result.Data != null)
            {
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _mediator.Send(new LoginCommand { Request = req });
            if (result.Success && result.Data != null)
            {
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            }
            return Ok(result);
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            var result = await _mediator.Send(new GoogleLoginCommand { Request = req });
            if (result.Success && result.Data != null)
            {
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            }
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            var result = await _mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken ?? "" });
            if (result.Success && result.Data != null)
            {
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            }
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            var result = await _mediator.Send(new LogoutCommand { RefreshToken = refreshToken ?? "" });
            ClearTokenCookies();
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst("email")?.Value;
            var fullName = User.FindFirst("fullName")?.Value;
            var roleId = User.FindFirst("role")?.Value; // In AuthService it's "role", not "roleId"

            return Ok(new
            {
                userId = userId != null ? long.Parse(userId) : 0,
                email,
                fullName,
                roleId = roleId != null ? int.Parse(roleId) : 0
            });
        }
    }
}
    