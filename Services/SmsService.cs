using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public class SmsService : ISmsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _encryptionKey;

        public SmsService(ApplicationDbContext context, IConfiguration configuration, ILogger<SmsService> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _encryptionKey = _configuration["SmsSettings:EncryptionKey"] ?? "DefaultSmsKey123456789012345678901234";
        }

        public async Task<SmsSettings?> GetSmsSettingsAsync()
        {
            return await _context.SmsSettings.FirstOrDefaultAsync();
        }

        public async Task<SmsSettings> CreateOrUpdateSmsSettingsAsync(SmsSettingsDto dto, string performedBy)
        {
            var existingSettings = await _context.SmsSettings.FirstOrDefaultAsync();
            
            if (existingSettings != null)
            {
                existingSettings.ProviderName = dto.ProviderName;
                existingSettings.ApiUrl = dto.ApiUrl;
                existingSettings.Username = dto.Username;
                existingSettings.Password = EncryptPassword(dto.Password);
                existingSettings.SenderName = dto.SenderName;
                existingSettings.Organisation = dto.Organisation;
                existingSettings.IsActive = dto.IsActive;
                existingSettings.UpdatedAt = DateTime.Now;
                existingSettings.UpdatedBy = performedBy;
                
                await _context.SaveChangesAsync();
                return existingSettings;
            }
            else
            {
                var newSettings = new SmsSettings
                {
                    ProviderName = dto.ProviderName,
                    ApiUrl = dto.ApiUrl,
                    Username = dto.Username,
                    Password = EncryptPassword(dto.Password),
                    SenderName = dto.SenderName,
                    Organisation = dto.Organisation,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = performedBy
                };
                
                _context.SmsSettings.Add(newSettings);
                await _context.SaveChangesAsync();
                return newSettings;
            }
        }

        public async Task<bool> SendVisitorNotificationAsync(string phoneNumber, string visitorName, string apartmentNumber)
        {
            try
            {
                var message = $"Ziyaretçi Bildirimi: {visitorName} isimli kişi {apartmentNumber} numaralı daireye ziyaret için geldi. Zaman: {DateTime.Now:dd.MM.yyyy HH:mm}";
                return await SendSmsAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending visitor notification SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendAlertAsync(string phoneNumber, string message)
        {
            try
            {
                var alertMessage = $"UYARI: {message} - {DateTime.Now:dd.MM.yyyy HH:mm}";
                return await SendSmsAsync(phoneNumber, alertMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, string? senderName = null)
        {
            var settings = await GetSmsSettingsAsync();
            if (settings == null || !settings.IsActive)
            {
                _logger.LogWarning("SMS settings not configured or inactive");
                return false;
            }

            var smsDto = new SmsSettingsDto
            {
                ProviderName = settings.ProviderName,
                ApiUrl = settings.ApiUrl,
                Username = settings.Username,
                Password = DecryptPassword(settings.Password),
                SenderName = senderName ?? settings.SenderName,
                Organisation = settings.Organisation,
                IsTestMode = settings.IsTestMode
            };

            return await SendSmsViaMutlucellAsync(smsDto, phoneNumber, message);
        }

        public async Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message, string? senderName = null)
        {
            var settings = await GetSmsSettingsAsync();
            if (settings == null || !settings.IsActive) return false;

            var smsDto = new SmsSettingsDto
            {
                ProviderName = settings.ProviderName,
                ApiUrl = settings.ApiUrl,
                Username = settings.Username,
                Password = DecryptPassword(settings.Password),
                SenderName = senderName ?? settings.SenderName,
                Organisation = settings.Organisation,
                IsTestMode = settings.IsTestMode
            };

            return await SendBulkSmsViaMutlucellAsync(smsDto, phoneNumbers, message);
        }

        public async Task<bool> SendTestSmsAsync(SmsSettingsDto settings, string phoneNumber, string message)
        {
            return await SendSmsViaMutlucellAsync(settings, phoneNumber, message);
        }

        public async Task<bool> TestSmsConnectionAsync(SmsSettingsDto settings)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(settings.ApiUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            await Task.CompletedTask;
            var phoneRegex = new Regex(@"^(\+90|90|0)?[5][0-9]{9}$");
            return phoneRegex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""));
        }

        public async Task<List<SmsPresetDto>> GetSmsPresetsAsync()
        {
            return await Task.FromResult(new List<SmsPresetDto>
            {
                new() {
                    Name = "Mutlucell",
                    ProviderName = "Mutlucell",
                    ApiUrl = "https://api.adresin.com/",
                    Description = "Mutlucell SMS Gateway - Türkiye'nin güvenilir SMS sağlayıcısı",
                    RequiresApiKey = false,
                    Documentation = "https://www.mutlucell.com.tr/4-hizmetler/9-toplu-sms/"
                },
                new() {
                    Name = "Netgsm",
                    ProviderName = "Netgsm", 
                    ApiUrl = "https://api.netgsm.com.tr/",
                    Description = "Netgsm SMS API",
                    RequiresApiKey = true,
                    Documentation = "https://www.netgsm.com.tr/dokuman/"
                }
            });
        }

        public async Task<int> GetRemainingDailyQuotaAsync()
        {
            var settings = await GetSmsSettingsAsync();
            return settings?.DailyLimit ?? 0;
        }

        public async Task<decimal> CalculateSmsCountAsync(string message)
        {
            await Task.CompletedTask;
            return Math.Ceiling((decimal)message.Length / 160);
        }

        public async Task<bool> DeactivateSmsSettingsAsync(string performedBy)
        {
            var settings = await GetSmsSettingsAsync();
            if (settings == null) return false;
            
            settings.IsActive = false;
            settings.UpdatedBy = performedBy;
            settings.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> SendSmsViaMutlucellAsync(SmsSettingsDto settings, string phoneNumber, string message)
        {
            try
            {
                if (settings.IsTestMode)
                {
                    _logger.LogInformation($"TEST MODE: SMS to {phoneNumber}: {message}");
                    return true;
                }

                var httpClient = _httpClientFactory.CreateClient();
                var parameters = new Dictionary<string, string>
                {
                    ["username"] = settings.Username,
                    ["password"] = settings.Password,
                    ["gsm"] = FormatPhoneNumber(phoneNumber),
                    ["message"] = message,
                    ["header"] = settings.SenderName
                };
                
                // Add organisation parameter if provided (for Mutlucell)
                if (!string.IsNullOrEmpty(settings.Organisation))
                {
                    parameters["organisation"] = settings.Organisation;
                }

                var content = new FormUrlEncodedContent(parameters);
                var response = await httpClient.PostAsync(settings.ApiUrl, content);
                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"SMS API Response: {responseText}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                return false;
            }
        }

        private async Task<bool> SendBulkSmsViaMutlucellAsync(SmsSettingsDto settings, List<string> phoneNumbers, string message)
        {
            var tasks = phoneNumbers.Select(phone => SendSmsViaMutlucellAsync(settings, phone, message));
            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }

        public string EncryptPassword(string password)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];
                
                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);
                
                swEncrypt.Write(password);
                swEncrypt.Close();
                
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch
            {
                return password;
            }
        }

        public string DecryptPassword(string encryptedPassword)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];
                
                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch
            {
                return encryptedPassword;
            }
        }

        public string FormatPhoneNumber(string phoneNumber)
        {
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "");
            if (cleaned.StartsWith("+90")) cleaned = cleaned.Substring(3);
            else if (cleaned.StartsWith("90")) cleaned = cleaned.Substring(2);
            else if (cleaned.StartsWith("0")) cleaned = cleaned.Substring(1);
            return "90" + cleaned;
        }
    }
}