namespace VisitorManagementSystem.Models.DTOs
{
    public class ResidentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string? Block { get; set; }
        public string? SubBlock { get; set; }
        public string? DoorNumber { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Original block and subblock for form editing
        public string? OriginalBlock { get; set; }
        public string? OriginalSubBlock { get; set; }
        
        public List<ResidentContactDto> Contacts { get; set; } = new();
        public List<ResidentVehicleDto> Vehicles { get; set; } = new();
    }
    
    public class ResidentContactDto
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ContactType { get; set; } = string.Empty; // Phone, Email
        public string ContactValue { get; set; } = string.Empty;
        public string? Label { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class ResidentVehicleDto
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? Year { get; set; }
        public string? VehicleType { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class CreateResidentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string? Block { get; set; }
        public string? SubBlock { get; set; }
        public string? DoorNumber { get; set; }
        public string? Notes { get; set; }
        
        public List<CreateResidentContactDto> Contacts { get; set; } = new();
        public List<CreateResidentVehicleDto> Vehicles { get; set; } = new();
    }
    
    public class CreateResidentContactDto
    {
        public string ContactType { get; set; } = string.Empty;
        public string ContactValue { get; set; } = string.Empty;
        public string? Label { get; set; }
        public int Priority { get; set; } = 1;
    }
    
    public class CreateResidentVehicleDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? Year { get; set; }
        public string? VehicleType { get; set; }
        public string? Notes { get; set; }
    }
    
    public class ResidentSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Block { get; set; }
        public string? SubBlock { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? ContactType { get; set; }
        public string? ContactValue { get; set; }
        public string? LicensePlate { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResidentDto
    {
        public List<ResidentDto> Residents { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}