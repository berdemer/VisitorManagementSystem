using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class SmsSettings
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = "Mutlucell"; // API Sağlayıcısı
        
        [Required]
        [StringLength(200)]
        public string ApiUrl { get; set; } = string.Empty; // API Endpoint URL
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty; // API Kullanıcı Adı
        
        [Required]
        [StringLength(500)] // Şifrelenmiş password için yeterli alan
        public string Password { get; set; } = string.Empty; // API Şifresi (Şifrelenmiş)
        
        [StringLength(100)]
        public string? ApiKey { get; set; } // Bazı API'ler key kullanır
        
        [Required]
        [StringLength(50)]
        public string SenderName { get; set; } = string.Empty; // Gönderen adı (11 karakter max)
        
        [StringLength(20)]
        public string? SenderNumber { get; set; } // Gönderen numara (alternatif)
        
        [StringLength(100)]
        public string? Organisation { get; set; } // Mutlucell için organizasyon
        
        public bool IsActive { get; set; } = false;
        
        // Test ayarları
        public bool IsTestMode { get; set; } = true; // Test modu
        [StringLength(15)]
        public string? TestPhoneNumber { get; set; } // Test için telefon
        
        // Quota ve limit ayarları
        public int DailyLimit { get; set; } = 100; // Günlük SMS limiti
        public decimal CostPerSms { get; set; } = 0.10m; // SMS başına maliyet
        
        // Audit alanları
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }
}