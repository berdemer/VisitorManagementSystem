using Microsoft.AspNetCore.Mvc;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsVerificationController : ControllerBase
    {
        private readonly ISmsVerificationService _smsVerificationService;
        private readonly ILogger<SmsVerificationController> _logger;

        public SmsVerificationController(
            ISmsVerificationService smsVerificationService,
            ILogger<SmsVerificationController> logger)
        {
            _smsVerificationService = smsVerificationService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendVerificationCode([FromBody] SendSmsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var code = await _smsVerificationService.SendVerificationCodeAsync(request.PhoneNumber);
                
                return Ok(new
                {
                    success = true,
                    message = "SMS kodu başarıyla gönderildi",
                    code = code, // For testing/display purposes
                    expiresAt = DateTime.UtcNow.AddMinutes(5)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS verification code to {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { success = false, message = "SMS gönderim hatası" });
            }
        }

        [HttpPost("verify")]
        public async Task<ActionResult> VerifyCode([FromBody] VerifySmsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var isValid = await _smsVerificationService.VerifyCodeAsync(request.PhoneNumber, request.Code);
                
                return Ok(new
                {
                    success = isValid,
                    message = isValid ? "Kod doğrulandı" : "Geçersiz kod"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying SMS code for {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { success = false, message = "Kod doğrulama hatası" });
            }
        }

        [HttpGet("status/{phoneNumber}")]
        public async Task<ActionResult> GetVerificationStatus(string phoneNumber)
        {
            try
            {
                var verification = await _smsVerificationService.GetActiveVerificationAsync(phoneNumber);
                
                if (verification == null)
                {
                    return Ok(new { hasActiveCode = false });
                }

                return Ok(new
                {
                    hasActiveCode = true,
                    code = verification.Code,
                    expiresAt = verification.ExpiresAt,
                    remainingMinutes = Math.Max(0, (int)(verification.ExpiresAt - DateTime.UtcNow).TotalMinutes)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification status for {PhoneNumber}", phoneNumber);
                return StatusCode(500, new { success = false, message = "Durum kontrol hatası" });
            }
        }
    }

    public class SendSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class VerifySmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}