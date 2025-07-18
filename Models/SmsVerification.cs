using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class SmsVerification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(3)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public string? CreatedBy { get; set; }
        
        // Helper properties
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsExpired && !IsUsed;
    }
}