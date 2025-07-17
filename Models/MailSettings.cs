using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class MailSettings
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string SenderEmail { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string SmtpServer { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 65535)]
        public int Port { get; set; } = 587;
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string SecurityType { get; set; } = "TLS"; // SSL, TLS, None
        
        public bool IsActive { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }
}