using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class Resident
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string ApartmentNumber { get; set; } = string.Empty;
        
        [StringLength(5)]
        public string? Block { get; set; } // Ana Blok (A, B, C, etc.)
        
        [StringLength(5)]
        public string? SubBlock { get; set; } // Alt Blok (1, 2, 3, etc.)
        
        [StringLength(10)]
        public string? DoorNumber { get; set; } // Kapı No
        
        [StringLength(200)]
        public string? Notes { get; set; } // Notlar
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
        public virtual ICollection<ResidentContact> Contacts { get; set; } = new List<ResidentContact>();
        public virtual ICollection<ResidentVehicle> Vehicles { get; set; } = new List<ResidentVehicle>();
    }
}