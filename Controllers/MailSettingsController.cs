using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailSettingsController : ControllerBase
    {
        private readonly IMailSettingsService _mailSettingsService;
        private readonly ILogger<MailSettingsController> _logger;

        public MailSettingsController(IMailSettingsService mailSettingsService, ILogger<MailSettingsController> logger)
        {
            _mailSettingsService = mailSettingsService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<MailSettings>> GetMailSettings()
        {
            try
            {
                var settings = await _mailSettingsService.GetMailSettingsAsync();
                
                if (settings == null)
                {
                    return Ok(new MailSettings()); // Return empty settings if none exist
                }

                // Don't return the actual password, return masked version
                settings.Password = new string('*', 8);
                
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mail settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MailSettings>> CreateOrUpdateMailSettings([FromBody] MailSettingsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var performedBy = User.Identity?.Name ?? "System";
                var settings = await _mailSettingsService.CreateOrUpdateMailSettingsAsync(dto, performedBy);
                
                // Don't return the actual password
                settings.Password = new string('*', 8);
                
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating mail settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("test-connection")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TestConnection([FromBody] MailSettingsDto settings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _mailSettingsService.TestMailConnectionAsync(settings);
                
                return Ok(new { success = result, message = result ? "Bağlantı başarılı" : "Bağlantı başarısız" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing mail connection");
                return Ok(new { success = false, message = "Bağlantı test edilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendTestMail([FromBody] SendTestMailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _mailSettingsService.SendTestMailAsync(request.Settings, request.TestMail);
                
                return Ok(new { 
                    success = result, 
                    message = result ? "Test maili başarıyla gönderildi" : "Test maili gönderilemedi" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test mail");
                return Ok(new { 
                    success = false, 
                    message = "Test maili gönderilirken hata oluştu: " + ex.Message 
                });
            }
        }

        [HttpPost("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateMailSettings()
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "System";
                var result = await _mailSettingsService.DeactivateMailSettingsAsync(performedBy);
                
                return Ok(new { 
                    success = result, 
                    message = result ? "Mail ayarları devre dışı bırakıldı" : "Mail ayarları bulunamadı" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating mail settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("presets")]
        [Authorize]
        public async Task<ActionResult<List<SmtpPresetDto>>> GetSmtpPresets()
        {
            try
            {
                var presets = await _mailSettingsService.GetSmtpPresetsAsync();
                return Ok(presets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMTP presets");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class SendTestMailRequest
    {
        public MailSettingsDto Settings { get; set; } = new();
        public TestMailDto TestMail { get; set; } = new();
    }
}