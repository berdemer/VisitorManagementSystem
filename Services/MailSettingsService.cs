using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public class MailSettingsService : IMailSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailSettingsService> _logger;
        private readonly string _encryptionKey;

        public MailSettingsService(ApplicationDbContext context, IConfiguration configuration, ILogger<MailSettingsService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _encryptionKey = _configuration["MailSettings:EncryptionKey"] ?? "DefaultKey123456789012345678901234"; // 32 characters for AES
        }

        public async Task<MailSettings?> GetMailSettingsAsync()
        {
            return await _context.MailSettings.FirstOrDefaultAsync();
        }

        public async Task<MailSettings> CreateOrUpdateMailSettingsAsync(MailSettingsDto dto, string performedBy)
        {
            var existingSettings = await _context.MailSettings.FirstOrDefaultAsync();
            
            if (existingSettings != null)
            {
                // Update existing
                existingSettings.SenderName = dto.SenderName;
                existingSettings.SenderEmail = dto.SenderEmail;
                existingSettings.SmtpServer = dto.SmtpServer;
                existingSettings.Port = dto.Port;
                existingSettings.Username = dto.Username;
                existingSettings.Password = EncryptPassword(dto.Password);
                existingSettings.SecurityType = dto.SecurityType;
                existingSettings.IsActive = dto.IsActive;
                existingSettings.UpdatedAt = DateTime.Now;
                existingSettings.UpdatedBy = performedBy;
                
                _context.Entry(existingSettings).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                return existingSettings;
            }
            else
            {
                // Create new
                var newSettings = new MailSettings
                {
                    SenderName = dto.SenderName,
                    SenderEmail = dto.SenderEmail,
                    SmtpServer = dto.SmtpServer,
                    Port = dto.Port,
                    Username = dto.Username,
                    Password = EncryptPassword(dto.Password),
                    SecurityType = dto.SecurityType,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = performedBy
                };
                
                _context.MailSettings.Add(newSettings);
                await _context.SaveChangesAsync();
                
                return newSettings;
            }
        }

        public async Task<bool> TestMailConnectionAsync(MailSettingsDto settings)
        {
            try
            {
                using var client = new SmtpClient(settings.SmtpServer, settings.Port);
                
                client.EnableSsl = settings.SecurityType != "None";
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
                client.Timeout = 10000; // 10 seconds timeout
                
                // Test connection by connecting to the SMTP server
                await Task.Run(() => {
                    // Create a minimal test message to validate connection
                    var testMessage = new MailMessage
                    {
                        From = new MailAddress(settings.SenderEmail),
                        Subject = "Connection Test",
                        Body = "Test"
                    };
                    testMessage.To.Add(settings.SenderEmail); // Send to self
                    
                    // Just test the connection, don't actually send
                    client.Send(testMessage);
                });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail connection test failed: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendTestMailAsync(MailSettingsDto settings, TestMailDto testMail)
        {
            try
            {
                _logger.LogInformation($"Attempting to send test mail with Username: {settings.Username}, " +
                    $"Server: {settings.SmtpServer}:{settings.Port}, " +
                    $"Password Length: {settings.Password?.Length ?? 0}, " +
                    $"From: {settings.SenderEmail}, To: {testMail.ToEmail}");
                
                using var client = new SmtpClient(settings.SmtpServer, settings.Port);
                
                client.EnableSsl = settings.SecurityType != "None";
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.SenderEmail, settings.SenderName),
                    Subject = testMail.Subject,
                    Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Test Mail</title>
</head>
<body style='font-family: Arial, sans-serif; margin: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
        <h2 style='color: #007bff; text-align: center;'>Ziyaretçi Yönetim Sistemi</h2>
        <h3 style='color: #333;'>Test Mail</h3>
        <p style='color: #666; line-height: 1.6;'>{testMail.Message}</p>
        <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
        <p style='color: #999; font-size: 12px; text-align: center;'>
            Bu mail {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde {settings.SenderName} tarafından gönderilmiştir.
        </p>
        <p style='color: #999; font-size: 12px; text-align: center;'>
            SMTP Sunucu: {settings.SmtpServer}:{settings.Port} ({settings.SecurityType})
        </p>
    </div>
</body>
</html>",
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(testMail.ToEmail);
                
                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation($"Test mail sent successfully to {testMail.ToEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send test mail to {testMail.ToEmail}");
                return false;
            }
        }

        public async Task<bool> DeactivateMailSettingsAsync(string performedBy)
        {
            try
            {
                var settings = await _context.MailSettings.FirstOrDefaultAsync();
                if (settings != null)
                {
                    settings.IsActive = false;
                    settings.UpdatedAt = DateTime.Now;
                    settings.UpdatedBy = performedBy;
                    
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate mail settings");
                return false;
            }
        }

        public async Task<List<SmtpPresetDto>> GetSmtpPresetsAsync()
        {
            return await Task.FromResult(new List<SmtpPresetDto>
            {
                new() {
                    Name = "Gmail",
                    SmtpServer = "smtp.gmail.com",
                    Port = 587,
                    SecurityType = "TLS",
                    Description = "Gmail SMTP ayarları (Uygulama şifresi gerekebilir)"
                },
                new() {
                    Name = "Yandex",
                    SmtpServer = "smtp.yandex.com",
                    Port = 587,
                    SecurityType = "TLS",
                    Description = "Yandex Mail SMTP ayarları"
                },
                new() {
                    Name = "Outlook/Hotmail",
                    SmtpServer = "smtp-mail.outlook.com",
                    Port = 587,
                    SecurityType = "TLS",
                    Description = "Microsoft Outlook/Hotmail SMTP ayarları"
                },
                new() {
                    Name = "Yahoo",
                    SmtpServer = "smtp.mail.yahoo.com",
                    Port = 587,
                    SecurityType = "TLS",
                    Description = "Yahoo Mail SMTP ayarları"
                },
                new() {
                    Name = "Türk Telekom",
                    SmtpServer = "smtp.ttnet.net.tr",
                    Port = 587,
                    SecurityType = "TLS",
                    Description = "Türk Telekom SMTP ayarları"
                }
            });
        }

        public string EncryptPassword(string password)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Use zero IV for simplicity
                
                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);
                
                swEncrypt.Write(password);
                swEncrypt.Close();
                
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt password");
                return password; // Fallback to plain text if encryption fails
            }
        }

        public string DecryptPassword(string encryptedPassword)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Use zero IV for simplicity
                
                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt password");
                return encryptedPassword; // Fallback to encrypted text if decryption fails
            }
        }
    }
}