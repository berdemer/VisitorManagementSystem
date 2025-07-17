using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class Visitor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string ApartmentNumber { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string IdNumber { get; set; } = string.Empty;
        
        public string? PhotoPath { get; set; }
        
        [Required]
        public DateTime CheckInTime { get; set; }
        
        public DateTime? CheckOutTime { get; set; }
        
        [StringLength(15)]
        public string? ResidentPhone { get; set; }
        
        [StringLength(100)]
        public string? ResidentName { get; set; }
        
        [StringLength(15)]
        public string? VisitorPhone { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
    }
}