using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models.DTOs
{
    public class SmsSettingsDto
    {
        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = "Mutlucell";
        
        [Required]
        [StringLength(200)]
        [Url]
        public string ApiUrl { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Password { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ApiKey { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SenderName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? SenderNumber { get; set; }
        
        [StringLength(100)]
        public string? Organisation { get; set; }
        
        public bool IsActive { get; set; } = false;
        public bool IsTestMode { get; set; } = true;
        
        [Phone]
        [StringLength(15)]
        public string? TestPhoneNumber { get; set; }
        
        [Range(1, 10000)]
        public int DailyLimit { get; set; } = 100;
        
        [Range(0, 999.99)]
        public decimal CostPerSms { get; set; } = 0.10m;
    }
    
    public class SendSmsDto
    {
        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? SenderName { get; set; }
        
        public bool IsUrgent { get; set; } = false;
        public DateTime? ScheduledTime { get; set; }
    }
    
    public class SendBulkSmsDto
    {
        [Required]
        [MinLength(1)]
        public List<string> PhoneNumbers { get; set; } = new();
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? SenderName { get; set; }
        
        public bool IsUrgent { get; set; } = false;
        public DateTime? ScheduledTime { get; set; }
    }
    
    public class SmsPresetDto
    {
        public string Name { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresApiKey { get; set; } = false;
        public string Documentation { get; set; } = string.Empty;
    }
}