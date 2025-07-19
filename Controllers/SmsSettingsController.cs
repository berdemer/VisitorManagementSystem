using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitorManagementSystem.Models.DTOs;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsSettingsController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly ILogger<SmsSettingsController> _logger;

        public SmsSettingsController(ISmsService smsService, ILogger<SmsSettingsController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetSmsSettings()
        {
            try
            {
                var settings = await _smsService.GetSmsSettingsAsync();
                
                if (settings == null)
                {
                    return Ok(new SmsSettingsDto()); // Return empty settings if none exist
                }

                // Don't return the actual password, return masked version
                var dto = new SmsSettingsDto
                {
                    ProviderName = settings.ProviderName,
                    ApiUrl = settings.ApiUrl,
                    Username = settings.Username,
                    Password = new string('*', 8), // Masked password
                    ApiKey = string.IsNullOrEmpty(settings.ApiKey) ? null : new string('*', 8),
                    SenderName = settings.SenderName,
                    SenderNumber = settings.SenderNumber,
                    Organisation = settings.Organisation,
                    IsActive = settings.IsActive,
                    IsTestMode = settings.IsTestMode,
                    TestPhoneNumber = settings.TestPhoneNumber,
                    DailyLimit = settings.DailyLimit,
                    CostPerSms = settings.CostPerSms
                };
                
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateOrUpdateSmsSettings([FromBody] SmsSettingsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var username = User.Identity?.Name ?? "Unknown";
                var result = await _smsService.CreateOrUpdateSmsSettingsAsync(dto, username);
                
                return Ok(new { 
                    success = true, 
                    message = "SMS ayarları başarıyla kaydedildi",
                    settings = new {
                        result.Id,
                        result.ProviderName,
                        result.IsActive,
                        result.IsTestMode
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating SMS settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("test-connection")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TestConnection([FromBody] SmsSettingsDto settings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Eğer şifre veritabanından geliyorsa decrypt et
                var existingSettings = await _smsService.GetSmsSettingsAsync();
                if (existingSettings != null && !string.IsNullOrEmpty(existingSettings.Password))
                {
                    try
                    {
                        var decryptedPassword = _smsService.DecryptPassword(existingSettings.Password);
                        settings.Password = decryptedPassword;
                    }
                    catch
                    {
                        // Decrypt başarısız olursa, DTO'daki şifreyi kullan
                    }
                }
                
                var result = await _smsService.TestSmsConnectionAsync(settings);
                
                return Ok(new { success = result, message = result ? "Bağlantı başarılı" : "Bağlantı başarısız" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS connection");
                return Ok(new { success = false, message = "Bağlantı test edilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendTestSms([FromBody] SendTestSmsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Eğer şifre veritabanından geliyorsa decrypt et
                var existingSettings = await _smsService.GetSmsSettingsAsync();
                if (existingSettings != null && !string.IsNullOrEmpty(existingSettings.Password))
                {
                    try
                    {
                        var decryptedPassword = _smsService.DecryptPassword(existingSettings.Password);
                        request.Settings.Password = decryptedPassword;
                    }
                    catch
                    {
                        // Decrypt başarısız olursa, DTO'daki şifreyi kullan
                    }
                }
                
                var result = await _smsService.SendTestSmsAsync(request.Settings, request.PhoneNumber, request.Message);
                
                return Ok(new { 
                    success = result, 
                    message = result ? "Test SMS başarıyla gönderildi" : "Test SMS gönderilemedi" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test SMS");
                return Ok(new { 
                    success = false, 
                    message = "Test SMS gönderilirken hata oluştu: " + ex.Message 
                });
            }
        }

        [HttpPost("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateSmsSettings()
        {
            try
            {
                var username = User.Identity?.Name ?? "Unknown";
                var result = await _smsService.DeactivateSmsSettingsAsync(username);
                
                return Ok(new { 
                    success = result, 
                    message = result ? "SMS ayarları deaktif edildi" : "SMS ayarları bulunamadı" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating SMS settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("presets")]
        [Authorize]
        public async Task<ActionResult> GetSmsPresets()
        {
            try
            {
                var presets = await _smsService.GetSmsPresetsAsync();
                return Ok(presets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS presets");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("quota")]
        [Authorize]
        public async Task<ActionResult> GetRemainingQuota()
        {
            try
            {
                var quota = await _smsService.GetRemainingDailyQuotaAsync();
                return Ok(new { remainingQuota = quota });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SMS quota");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("validate-phone")]
        [Authorize]
        public async Task<ActionResult> ValidatePhoneNumber([FromBody] ValidatePhoneRequest request)
        {
            try
            {
                var isValid = await _smsService.ValidatePhoneNumberAsync(request.PhoneNumber);
                var formatted = _smsService.FormatPhoneNumber(request.PhoneNumber);
                
                return Ok(new { 
                    isValid = isValid, 
                    formattedNumber = formatted,
                    message = isValid ? "Geçerli telefon numarası" : "Geçersiz telefon numarası formatı"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating phone number");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    // Request DTOs
    public class SendTestSmsRequest
    {
        public SmsSettingsDto Settings { get; set; } = new();
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ValidatePhoneRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }
}