using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public interface ISmsService
    {
        // Settings management
        Task<SmsSettings?> GetSmsSettingsAsync();
        Task<SmsSettings> CreateOrUpdateSmsSettingsAsync(SmsSettingsDto dto, string performedBy);
        Task<bool> DeactivateSmsSettingsAsync(string performedBy);
        
        // SMS operations
        Task<bool> SendSmsAsync(string phoneNumber, string message, string? senderName = null);
        Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message, string? senderName = null);
        Task<bool> SendTestSmsAsync(SmsSettingsDto settings, string phoneNumber, string message);
        Task<bool> SendVisitorNotificationAsync(string phoneNumber, string visitorName, string apartmentNumber);
        Task<bool> SendAlertAsync(string phoneNumber, string message);
        
        // Connection and validation
        Task<bool> TestSmsConnectionAsync(SmsSettingsDto settings);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
        
        // Provider presets
        Task<List<SmsPresetDto>> GetSmsPresetsAsync();
        
        // Statistics and reporting
        Task<int> GetRemainingDailyQuotaAsync();
        Task<decimal> CalculateSmsCountAsync(string message);
        
        // Utility methods
        string EncryptPassword(string password);
        string DecryptPassword(string encryptedPassword);
        string FormatPhoneNumber(string phoneNumber);
    }
}