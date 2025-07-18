using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public interface IVisitorService
    {
        Task<IEnumerable<Visitor>> GetAllVisitorsAsync();
        Task<IEnumerable<Visitor>> GetActiveVisitorsAsync();
        Task<Visitor?> GetVisitorByIdAsync(int id);
        Task<Visitor> CreateVisitorAsync(Visitor visitor);
        Task<Visitor> UpdateVisitorAsync(Visitor visitor);
        Task<bool> CheckOutVisitorAsync(int id, string performedBy);
        Task<IEnumerable<Visitor>> GetVisitorsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Visitor>> GetVisitorsByApartmentAsync(string apartmentNumber);
        Task<bool> DeleteVisitorAsync(int id);
        Task<byte[]> ExportVisitorsToExcelAsync(IEnumerable<Visitor> visitors, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ApartmentVisitStatDto>> GetMostVisitedApartmentsAsync(DateTime? startDate, DateTime? endDate);
        
        // Pagination methods
        Task<PagedVisitorDto> GetVisitorsPagedAsync(int page = 1, int pageSize = 10);
        Task<PagedVisitorDto> SearchVisitorsAsync(VisitorSearchDto searchDto);
        
        // Search visitors by name for autocomplete
        Task<IEnumerable<Visitor>> SearchVisitorsByNameAsync(string name);
    }

    public class ApartmentVisitStatDto
    {
        public string ApartmentNumber { get; set; } = string.Empty;
        public int VisitorCount { get; set; }
        public int ActiveVisitorCount { get; set; }
        public DateTime LastVisitDate { get; set; }
        public string MostFrequentVisitor { get; set; } = string.Empty;
    }
}