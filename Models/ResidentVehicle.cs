using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.Models
{
    public class ResidentVehicle
    {
        public int Id { get; set; }
        
        [Required]
        public int ResidentId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Brand { get; set; } // Marka
        
        [StringLength(50)]
        public string? Model { get; set; } // Model
        
        [StringLength(20)]
        public string? Color { get; set; } // Renk
        
        [StringLength(4)]
        public string? Year { get; set; } // Model yılı
        
        [StringLength(50)]
        public string? VehicleType { get; set; } // Otomobil, Motosiklet, vs.
        
        [StringLength(200)]
        public string? Notes { get; set; } // Notlar
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public virtual Resident Resident { get; set; } = null!;
    }
}