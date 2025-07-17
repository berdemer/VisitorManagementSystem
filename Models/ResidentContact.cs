using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class ResidentContact
    {
        public int Id { get; set; }
        
        [Required]
        public int ResidentId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ContactType { get; set; } = string.Empty; // Phone, Email
        
        [Required]
        [StringLength(100)]
        public string ContactValue { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Label { get; set; } // Ev, İş, Mobil, vs.
        
        public int Priority { get; set; } = 1; // 1 = Yüksek, 2 = Orta, 3 = Düşük
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public virtual Resident Resident { get; set; } = null!;
    }
}