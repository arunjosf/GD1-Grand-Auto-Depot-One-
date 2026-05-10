using GD1.Application.Features.Auth.Commands;
using GD1.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GD1.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;


        [HttpPost("register")]
        [AllowAnonymous]    
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _mediator.Send(new RegisterCommand { Request = req });
            if (result.Success && result.Data != null)
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _mediator.Send(new LoginCommand { Request = req });
            if (result.Success && result.Data != null)
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(result);
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            var result = await _mediator.Send(new GoogleLoginCommand { Request = req });
            if (result.Success && result.Data != null)
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(result);
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest req)
        {
            var result = await _mediator.Send(
                new VerifyEmailOtpCommand { Request = req });
            if (result.Success && result.Data != null)
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(result);
        }


        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            var result = await _mediator.Send(
                new RefreshTokenCommand { RefreshToken = refreshToken ?? "" });
            if (result.Success && result.Data != null)
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            var result = await _mediator.Send(
                new LogoutCommand { RefreshToken = refreshToken ?? "" });
            ClearTokenCookies();
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me([FromServices] GD1.Infrastructure.Data.AppDbContext db)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                              ?? User.FindFirst("userId")?.Value;

            if (userIdClaim == null) return Unauthorized();

            var userId = long.Parse(userIdClaim);
            var user = await db.Users.FindAsync(userId);

            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role.ToString(),
                roleId = (int)user.Role
            });
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("AccessToken", accessToken, opts);
            Response.Cookies.Append("RefreshToken", refreshToken, opts);
        }

        private void ClearTokenCookies()
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
        }
    }
}