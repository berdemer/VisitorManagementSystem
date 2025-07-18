using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services
{
    public interface ISmsVerificationService
    {
        Task<string> SendVerificationCodeAsync(string phoneNumber, string? userId = null);
        Task<bool> VerifyCodeAsync(string phoneNumber, string code);
        Task<SmsVerification?> GetActiveVerificationAsync(string phoneNumber);
        Task InvalidateCodesAsync(string phoneNumber);
        Task CleanupExpiredCodesAsync();
    }
}