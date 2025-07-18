using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services
{
    public class SmsVerificationService : ISmsVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger<SmsVerificationService> _logger;

        public SmsVerificationService(
            ApplicationDbContext context,
            ISmsService smsService,
            ILogger<SmsVerificationService> logger)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<string> SendVerificationCodeAsync(string phoneNumber, string? userId = null)
        {
            // Clean phone number
            phoneNumber = CleanPhoneNumber(phoneNumber);
            
            // Check rate limiting - 1 minute between requests
            var recentCode = await _context.SmsVerifications
                .Where(v => v.PhoneNumber == phoneNumber)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentCode != null && recentCode.CreatedAt.AddMinutes(1) > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Bu telefon numarasına 1 dakika içinde tekrar kod gönderilemez");
            }

            // Generate 3-digit code
            var code = GenerateCode();
            
            // Invalidate previous codes
            await InvalidateCodesAsync(phoneNumber);

            // Create new verification record
            var verification = new SmsVerification
            {
                PhoneNumber = phoneNumber,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CreatedBy = userId ?? "System"
            };

            _context.SmsVerifications.Add(verification);
            await _context.SaveChangesAsync();

            // Send SMS
            var message = $"[Ziyaretçi Giriş Kodu]\n\nGiriş işleminizi tamamlamak için bu kodu güvenlik görevlisine gösteriniz: {code}";
            
            try
            {
                await _smsService.SendSmsAsync(phoneNumber, message);
                _logger.LogInformation("SMS verification code sent to {PhoneNumber}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                // Even if SMS fails, return the code for display
                // throw; // Don't throw, let the UI show the code
            }

            return code;
        }

        public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
        {
            phoneNumber = CleanPhoneNumber(phoneNumber);
            
            var verification = await GetActiveVerificationAsync(phoneNumber);
            
            if (verification == null || verification.Code != code)
            {
                return false;
            }

            // Mark as used
            verification.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<SmsVerification?> GetActiveVerificationAsync(string phoneNumber)
        {
            phoneNumber = CleanPhoneNumber(phoneNumber);
            
            return await _context.SmsVerifications
                .Where(v => v.PhoneNumber == phoneNumber && v.IsValid)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateCodesAsync(string phoneNumber)
        {
            phoneNumber = CleanPhoneNumber(phoneNumber);
            
            var activeCodes = await _context.SmsVerifications
                .Where(v => v.PhoneNumber == phoneNumber && !v.IsUsed)
                .ToListAsync();

            foreach (var code in activeCodes)
            {
                code.IsUsed = true;
            }

            if (activeCodes.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredCodesAsync()
        {
            var expiredCodes = await _context.SmsVerifications
                .Where(v => v.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.SmsVerifications.RemoveRange(expiredCodes);
            
            if (expiredCodes.Count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired SMS verification codes", expiredCodes.Count);
            }
        }

        private string GenerateCode()
        {
            var random = new Random();
            return random.Next(100, 999).ToString();
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            // Remove all non-digits
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Remove leading country code if present
            if (cleaned.StartsWith("90") && cleaned.Length == 12)
            {
                cleaned = cleaned.Substring(2);
            }
            
            // Add leading 0 if missing
            if (!cleaned.StartsWith("0") && cleaned.Length == 10)
            {
                cleaned = "0" + cleaned;
            }
            
            return cleaned;
        }
    }
}