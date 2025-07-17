namespace VisitorManagementSystem.Models.DTOs
{
    public class VisitorSearchDto
    {
        public string? Date { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? LicensePlate { get; set; }
        public string? Status { get; set; } // "active", "inactive", ""
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedVisitorDto
    {
        public List<VisitorDto> Visitors { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class VisitorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? IdNumber { get; set; }
        public string ApartmentNumber { get; set; } = string.Empty;
        public string? LicensePlate { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool IsActive { get; set; }
        public string? PhotoPath { get; set; }
        public string? Notes { get; set; }
        public string? ResidentName { get; set; }
        public string? ResidentPhone { get; set; }
        public string? VisitorPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}