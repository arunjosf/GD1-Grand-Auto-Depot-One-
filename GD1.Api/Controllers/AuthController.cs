using GD1.Application.Features.Auth.Commands;
using GD1.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegisterCommandHandler _registerHandler;
        private readonly LoginCommandHandler _loginHandler;
        private readonly GoogleLoginCommandHandler _googleHandler;
        private readonly RefreshTokenCommandHandler _refreshHandler;
        private readonly LogoutCommandHandler _logoutHandler;

        public AuthController(
            RegisterCommandHandler registerHandler,
            LoginCommandHandler loginHandler,
            GoogleLoginCommandHandler googleHandler,
            RefreshTokenCommandHandler refreshHandler,
            LogoutCommandHandler logoutHandler)
        {
            _registerHandler = registerHandler;
            _loginHandler = loginHandler;
            _googleHandler = googleHandler;
            _refreshHandler = refreshHandler;
            _logoutHandler = logoutHandler;
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _registerHandler.HandleAsync(
                new RegisterCommand { Request = req });
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _loginHandler.HandleAsync(
                new LoginCommand { Request = req });
            return Ok(result);
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            var result = await _googleHandler.HandleAsync(
                new GoogleLoginCommand { Request = req });
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
        {
            var result = await _refreshHandler.HandleAsync(
                new RefreshTokenCommand { RefreshToken = req.RefreshToken });
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req)
        {
            var result = await _logoutHandler.HandleAsync(
                new LogoutCommand { RefreshToken = req.RefreshToken });
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst("email")?.Value;
            var fullName = User.FindFirst("fullName")?.Value;
            var roleId = User.FindFirst("roleId")?.Value;

            return Ok(new
            {
                userId,
                email,
                fullName,
                roleId = int.Parse(roleId ?? "0")
            });
        }
    }
}
    