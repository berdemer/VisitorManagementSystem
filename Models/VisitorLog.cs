using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class VisitorLog
    {
        public int Id { get; set; }
        
        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CheckIn, CheckOut, PhotoTaken, SMSSent
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [StringLength(200)]
        public string? Details { get; set; }
        
        public string PerformedBy { get; set; } = string.Empty;
    }
}