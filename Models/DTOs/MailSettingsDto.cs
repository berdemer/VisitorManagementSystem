using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models.DTOs
{
    public class MailSettingsDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Gönderen adı gereklidir")]
        [StringLength(100, ErrorMessage = "Gönderen adı en fazla 100 karakter olmalıdır")]
        public string SenderName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olmalıdır")]
        public string SenderEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "SMTP sunucusu gereklidir")]
        [StringLength(100, ErrorMessage = "SMTP sunucusu en fazla 100 karakter olmalıdır")]
        public string SmtpServer { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Port numarası gereklidir")]
        [Range(1, 65535, ErrorMessage = "Port numarası 1-65535 arasında olmalıdır")]
        public int Port { get; set; } = 587;
        
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(100, ErrorMessage = "Kullanıcı adı en fazla 100 karakter olmalıdır")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre gereklidir")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Güvenlik türü gereklidir")]
        public string SecurityType { get; set; } = "TLS";
        
        public bool IsActive { get; set; } = false;
    }

    public class TestMailDto
    {
        [Required(ErrorMessage = "Alıcı e-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string ToEmail { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olmalıdır")]
        public string Subject { get; set; } = "Test Mail - Ziyaretçi Yönetim Sistemi";
        
        [StringLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olmalıdır")]
        public string Message { get; set; } = "Bu bir test mesajıdır. Mail ayarlarınız başarıyla çalışıyor.";
    }

    public class SmtpPresetDto
    {
        public string Name { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SecurityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}