using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.AuthenticateAsync(request.Username, request.Password);
                if (user == null)
                    return Unauthorized(new { message = "Invalid username or password" });

                var token = await _userService.GenerateJwtTokenAsync(user);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var success = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                
                if (!success)
                    return BadRequest(new { message = "Current password is incorrect" });

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}