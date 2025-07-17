using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Security"; // Security, Admin, Manager
        
        public bool IsActive { get; set; } = true;
        
        public DateTime LastLogin { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}