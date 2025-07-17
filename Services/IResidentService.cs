using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;

namespace VisitorManagementSystem.Services
{
    public interface IResidentService
    {
        // Resident CRUD operations
        Task<ResidentDto> GetResidentByIdAsync(int id);
        Task<List<ResidentDto>> GetAllResidentsAsync();
        Task<PagedResidentDto> GetResidentsPagedAsync(int page = 1, int pageSize = 10);
        Task<PagedResidentDto> SearchResidentsAsync(ResidentSearchDto searchDto);
        Task<ResidentDto> CreateResidentAsync(CreateResidentDto createDto);
        Task<ResidentDto> UpdateResidentAsync(int id, CreateResidentDto updateDto);
        Task<bool> DeleteResidentAsync(int id);
        
        // Contact operations
        Task<ResidentContactDto> AddContactAsync(int residentId, CreateResidentContactDto contactDto);
        Task<ResidentContactDto> UpdateContactAsync(int contactId, CreateResidentContactDto contactDto);
        Task<bool> DeleteContactAsync(int contactId);
        Task<List<ResidentContactDto>> GetContactsByResidentAsync(int residentId);
        
        // Vehicle operations
        Task<ResidentVehicleDto> AddVehicleAsync(int residentId, CreateResidentVehicleDto vehicleDto);
        Task<ResidentVehicleDto> UpdateVehicleAsync(int vehicleId, CreateResidentVehicleDto vehicleDto);
        Task<bool> DeleteVehicleAsync(int vehicleId);
        Task<List<ResidentVehicleDto>> GetVehiclesByResidentAsync(int residentId);
        
        // Search operations
        Task<List<ResidentDto>> SearchByContactAsync(string contactValue);
        Task<List<ResidentDto>> SearchByLicensePlateAsync(string licensePlate);
        Task<ResidentDto?> GetResidentByApartmentAsync(string apartmentNumber);
        
        // Bulk operations
        Task<List<ResidentDto>> BulkCreateResidentsAsync(List<CreateResidentDto> residents);
        Task<byte[]> ExportResidentsToExcelAsync();
        Task<List<string>> ImportResidentsFromExcelAsync(byte[] excelData);
        
        // Priority operations
        Task<List<ResidentContactDto>> GetPriorityContactsAsync(int residentId, string contactType);
        Task<ResidentContactDto?> GetHighestPriorityContactAsync(int residentId, string contactType);
    }
}