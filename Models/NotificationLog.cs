using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string NotificationType { get; set; } = string.Empty; // "SMS" or "EMAIL"
        
        [MaxLength(10)]
        public string? ApartmentNumber { get; set; }
        
        [MaxLength(200)]
        public string? VisitorName { get; set; }
        
        [MaxLength(20)]
        public string? VisitorPhone { get; set; }
        
        [MaxLength(200)]
        public string? RecipientEmail { get; set; }
        
        [MaxLength(20)]
        public string? RecipientPhone { get; set; }
        
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // "SUCCESS", "FAILED", "PENDING"
        
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
        
        [MaxLength(100)]
        public string? ExternalId { get; set; } // SMS ID veya Mail tracking ID
        
        public DateTime SentAt { get; set; }
        
        [MaxLength(100)]
        public string? SentBy { get; set; } // Kullanıcı adı
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public int? VisitorId { get; set; }
        public virtual Visitor? Visitor { get; set; }
        
        public int? ResidentId { get; set; }
        public virtual Resident? Resident { get; set; }
    }
}