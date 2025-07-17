using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public interface IMailSettingsService
    {
        Task<MailSettings?> GetMailSettingsAsync();
        Task<MailSettings> CreateOrUpdateMailSettingsAsync(MailSettingsDto dto, string performedBy);
        Task<bool> TestMailConnectionAsync(MailSettingsDto settings);
        Task<bool> SendTestMailAsync(MailSettingsDto settings, TestMailDto testMail);
        Task<bool> DeactivateMailSettingsAsync(string performedBy);
        Task<List<SmtpPresetDto>> GetSmtpPresetsAsync();
        string EncryptPassword(string password);
        string DecryptPassword(string encryptedPassword);
    }
}