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
                var userIdClaim = User.FindFirst("sub")?.Value;
                Console.WriteLine($"JWT sub claim: '{userIdClaim}'");
                
                var userId = int.Parse(userIdClaim ?? "0");
                Console.WriteLine($"Parsed user ID: {userId}");
                _logger.LogInformation($"Change password request for user ID: {userId}");
                
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

        [HttpPost("test-hash")]
        [AllowAnonymous]
        public ActionResult TestHash([FromBody] TestHashRequest request)
        {
            try
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var verify = BCrypt.Net.BCrypt.Verify(request.Password, hash);
                
                return Ok(new { 
                    password = request.Password,
                    hash = hash,
                    verify = verify 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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

    public class TestHashRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}