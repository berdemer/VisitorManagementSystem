using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;
using OfficeOpenXml;
using System.Text;

namespace VisitorManagementSystem.Services
{
    public class ResidentService : IResidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResidentService> _logger;

        public ResidentService(ApplicationDbContext context, ILogger<ResidentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ResidentDto> GetResidentByIdAsync(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

            if (resident == null)
                throw new ArgumentException("Resident not found");

            return MapToDto(resident);
        }

        public async Task<List<ResidentDto>> GetAllResidentsAsync()
        {
            var residents = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive)
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => r.DoorNumber)
                .ToListAsync();

            return residents.Select(MapToDto).ToList();
        }

        public async Task<PagedResidentDto> GetResidentsPagedAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive);

            var totalCount = await query.CountAsync();

            var allResidents = await query
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => r.DoorNumber)
                .ToListAsync();

            // Sort by door number numerically in memory
            var sortedResidents = allResidents
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => int.TryParse(r.DoorNumber, out int doorNum) ? doorNum : 0)
                .ToList();

            var residents = sortedResidents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResidentDto
            {
                Residents = residents.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResidentDto> SearchResidentsAsync(ResidentSearchDto searchDto)
        {
            var query = _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive);

            // Apply non-text search filters first (these can be done in SQL)
            if (!string.IsNullOrEmpty(searchDto.Block))
                query = query.Where(r => r.Block == searchDto.Block);

            if (!string.IsNullOrEmpty(searchDto.SubBlock))
                query = query.Where(r => r.SubBlock == searchDto.SubBlock);

            if (!string.IsNullOrEmpty(searchDto.ApartmentNumber))
                query = query.Where(r => r.ApartmentNumber.Contains(searchDto.ApartmentNumber));

            if (!string.IsNullOrEmpty(searchDto.ContactType) && !string.IsNullOrEmpty(searchDto.ContactValue))
            {
                query = query.Where(r => r.Contacts.Any(c => 
                    c.ContactType == searchDto.ContactType && 
                    c.ContactValue.Contains(searchDto.ContactValue)
                ));
            }

            if (!string.IsNullOrEmpty(searchDto.LicensePlate))
            {
                query = query.Where(r => r.Vehicles.Any(v => 
                    v.LicensePlate.Contains(searchDto.LicensePlate)
                ));
            }

            // Get all matching residents first, then filter in memory for Turkish characters
            var allResidents = await query
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => r.DoorNumber)
                .ToListAsync();

            // Apply text search with Turkish character normalization in memory
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchTerm = NormalizeTurkishText(searchDto.SearchTerm.ToLower().Trim());
                var searchKeywords = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                allResidents = allResidents.Where(r =>
                {
                    foreach (var keyword in searchKeywords)
                    {
                        bool found = NormalizeTurkishText(r.FullName.ToLower()).Contains(keyword) ||
                                   NormalizeTurkishText(r.ApartmentNumber.ToLower()).Contains(keyword) ||
                                   r.Contacts.Any(c => NormalizeTurkishText(c.ContactValue.ToLower()).Contains(keyword)) ||
                                   r.Vehicles.Any(v => NormalizeTurkishText(v.LicensePlate.ToLower()).Contains(keyword));
                        
                        if (!found) return false;
                    }
                    return true;
                }).ToList();
            }

            // Sort by door number numerically in memory
            var sortedResidents = allResidents
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => int.TryParse(r.DoorNumber, out int doorNum) ? doorNum : 0)
                .ToList();

            // Apply pagination in memory
            var totalCount = sortedResidents.Count;
            var residents = sortedResidents
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToList();

            return new PagedResidentDto
            {
                Residents = residents.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<ResidentDto> CreateResidentAsync(CreateResidentDto createDto)
        {
            // Check if apartment number already exists (case insensitive, all records)
            var existingResident = await _context.Residents
                .FirstOrDefaultAsync(r => r.ApartmentNumber.ToLower() == createDto.ApartmentNumber.ToLower());

            if (existingResident != null)
            {
                if (existingResident.IsActive)
                {
                    throw new ArgumentException($"Bu daire numarası zaten aktif kayıt olarak mevcut - {existingResident.FullName}");
                }
                else
                {
                    throw new ArgumentException($"Bu daire numarası daha önce kullanılmış (pasif kayıt) - {existingResident.FullName}");
                }
            }

            var resident = new Resident
            {
                FullName = createDto.FullName,
                ApartmentNumber = createDto.ApartmentNumber,
                Block = createDto.Block,
                SubBlock = createDto.SubBlock,
                DoorNumber = createDto.DoorNumber,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            // Add contacts
            foreach (var contactDto in createDto.Contacts)
            {
                var contact = new ResidentContact
                {
                    ResidentId = resident.Id,
                    ContactType = contactDto.ContactType,
                    ContactValue = contactDto.ContactValue,
                    Label = contactDto.Label,
                    Priority = contactDto.Priority,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ResidentContacts.Add(contact);
            }

            // Add vehicles
            foreach (var vehicleDto in createDto.Vehicles)
            {
                var vehicle = new ResidentVehicle
                {
                    ResidentId = resident.Id,
                    LicensePlate = vehicleDto.LicensePlate,
                    Brand = vehicleDto.Brand,
                    Model = vehicleDto.Model,
                    Color = vehicleDto.Color,
                    Year = vehicleDto.Year,
                    VehicleType = vehicleDto.VehicleType,
                    Notes = vehicleDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ResidentVehicles.Add(vehicle);
            }

            await _context.SaveChangesAsync();

            return await GetResidentByIdAsync(resident.Id);
        }

        public async Task<ResidentDto> UpdateResidentAsync(int id, CreateResidentDto updateDto)
        {
            var resident = await _context.Residents
                .Include(r => r.Contacts)
                .Include(r => r.Vehicles)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

            if (resident == null)
                throw new ArgumentException("Resident not found");

            // Check if apartment number conflicts with another resident (case insensitive, all records)
            var existingResident = await _context.Residents
                .FirstOrDefaultAsync(r => r.ApartmentNumber.ToLower() == updateDto.ApartmentNumber.ToLower() && r.Id != id);

            if (existingResident != null)
            {
                if (existingResident.IsActive)
                {
                    throw new ArgumentException($"Bu daire numarası zaten aktif kayıt olarak mevcut - {existingResident.FullName}");
                }
                else
                {
                    throw new ArgumentException($"Bu daire numarası daha önce kullanılmış (pasif kayıt) - {existingResident.FullName}");
                }
            }

            // Update resident basic info
            resident.FullName = updateDto.FullName;
            resident.ApartmentNumber = updateDto.ApartmentNumber;
            resident.Block = updateDto.Block;
            resident.SubBlock = updateDto.SubBlock;
            resident.DoorNumber = updateDto.DoorNumber;
            resident.Notes = updateDto.Notes;
            resident.UpdatedAt = DateTime.UtcNow;

            // Update contacts - soft delete existing ones and add new ones
            var existingContacts = resident.Contacts.Where(c => c.IsActive).ToList();
            foreach (var contact in existingContacts)
            {
                contact.IsActive = false;
                contact.UpdatedAt = DateTime.UtcNow;
            }

            // Add new contacts
            foreach (var contactDto in updateDto.Contacts)
            {
                var contact = new ResidentContact
                {
                    ResidentId = id,
                    ContactType = contactDto.ContactType,
                    ContactValue = contactDto.ContactValue,
                    Label = contactDto.Label,
                    Priority = contactDto.Priority,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ResidentContacts.Add(contact);
            }

            // Update vehicles - soft delete existing ones and add new ones
            var existingVehicles = resident.Vehicles.Where(v => v.IsActive).ToList();
            foreach (var vehicle in existingVehicles)
            {
                vehicle.IsActive = false;
                vehicle.UpdatedAt = DateTime.UtcNow;
            }

            // Add new vehicles
            foreach (var vehicleDto in updateDto.Vehicles)
            {
                var vehicle = new ResidentVehicle
                {
                    ResidentId = id,
                    LicensePlate = vehicleDto.LicensePlate,
                    Brand = vehicleDto.Brand,
                    Model = vehicleDto.Model,
                    Color = vehicleDto.Color,
                    Year = vehicleDto.Year,
                    VehicleType = vehicleDto.VehicleType,
                    Notes = vehicleDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ResidentVehicles.Add(vehicle);
            }

            await _context.SaveChangesAsync();

            return await GetResidentByIdAsync(id);
        }

        public async Task<bool> DeleteResidentAsync(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null || !resident.IsActive)
                return false;

            resident.IsActive = false;
            resident.UpdatedAt = DateTime.UtcNow;

            // Soft delete contacts and vehicles
            var contacts = await _context.ResidentContacts
                .Where(c => c.ResidentId == id && c.IsActive)
                .ToListAsync();

            foreach (var contact in contacts)
            {
                contact.IsActive = false;
                contact.UpdatedAt = DateTime.UtcNow;
            }

            var vehicles = await _context.ResidentVehicles
                .Where(v => v.ResidentId == id && v.IsActive)
                .ToListAsync();

            foreach (var vehicle in vehicles)
            {
                vehicle.IsActive = false;
                vehicle.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResidentContactDto> AddContactAsync(int residentId, CreateResidentContactDto contactDto)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || !resident.IsActive)
                throw new ArgumentException("Resident not found");

            var contact = new ResidentContact
            {
                ResidentId = residentId,
                ContactType = contactDto.ContactType,
                ContactValue = contactDto.ContactValue,
                Label = contactDto.Label,
                Priority = contactDto.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.ResidentContacts.Add(contact);
            await _context.SaveChangesAsync();

            return MapContactToDto(contact);
        }

        public async Task<ResidentContactDto> UpdateContactAsync(int contactId, CreateResidentContactDto contactDto)
        {
            var contact = await _context.ResidentContacts.FindAsync(contactId);
            if (contact == null || !contact.IsActive)
                throw new ArgumentException("Contact not found");

            contact.ContactType = contactDto.ContactType;
            contact.ContactValue = contactDto.ContactValue;
            contact.Label = contactDto.Label;
            contact.Priority = contactDto.Priority;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapContactToDto(contact);
        }

        public async Task<bool> DeleteContactAsync(int contactId)
        {
            var contact = await _context.ResidentContacts.FindAsync(contactId);
            if (contact == null || !contact.IsActive)
                return false;

            contact.IsActive = false;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ResidentContactDto>> GetContactsByResidentAsync(int residentId)
        {
            var contacts = await _context.ResidentContacts
                .Where(c => c.ResidentId == residentId && c.IsActive)
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.ContactType)
                .ToListAsync();

            return contacts.Select(MapContactToDto).ToList();
        }

        public async Task<ResidentVehicleDto> AddVehicleAsync(int residentId, CreateResidentVehicleDto vehicleDto)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null || !resident.IsActive)
                throw new ArgumentException("Resident not found");

            var vehicle = new ResidentVehicle
            {
                ResidentId = residentId,
                LicensePlate = vehicleDto.LicensePlate,
                Brand = vehicleDto.Brand,
                Model = vehicleDto.Model,
                Color = vehicleDto.Color,
                Year = vehicleDto.Year,
                VehicleType = vehicleDto.VehicleType,
                Notes = vehicleDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.ResidentVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return MapVehicleToDto(vehicle);
        }

        public async Task<ResidentVehicleDto> UpdateVehicleAsync(int vehicleId, CreateResidentVehicleDto vehicleDto)
        {
            var vehicle = await _context.ResidentVehicles.FindAsync(vehicleId);
            if (vehicle == null || !vehicle.IsActive)
                throw new ArgumentException("Vehicle not found");

            vehicle.LicensePlate = vehicleDto.LicensePlate;
            vehicle.Brand = vehicleDto.Brand;
            vehicle.Model = vehicleDto.Model;
            vehicle.Color = vehicleDto.Color;
            vehicle.Year = vehicleDto.Year;
            vehicle.VehicleType = vehicleDto.VehicleType;
            vehicle.Notes = vehicleDto.Notes;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapVehicleToDto(vehicle);
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId)
        {
            var vehicle = await _context.ResidentVehicles.FindAsync(vehicleId);
            if (vehicle == null || !vehicle.IsActive)
                return false;

            vehicle.IsActive = false;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ResidentVehicleDto>> GetVehiclesByResidentAsync(int residentId)
        {
            var vehicles = await _context.ResidentVehicles
                .Where(v => v.ResidentId == residentId && v.IsActive)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();

            return vehicles.Select(MapVehicleToDto).ToList();
        }

        public async Task<List<ResidentDto>> SearchByContactAsync(string contactValue)
        {
            var residents = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive && r.Contacts.Any(c => c.ContactValue.Contains(contactValue)))
                .ToListAsync();

            return residents.Select(MapToDto).ToList();
        }

        public async Task<List<ResidentDto>> SearchByLicensePlateAsync(string licensePlate)
        {
            var searchTerm = licensePlate.ToUpper().Trim();
            
            var residents = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive && r.Vehicles.Any(v => v.IsActive && v.LicensePlate.ToUpper().Contains(searchTerm)))
                .ToListAsync();

            return residents.Select(MapToDto).ToList();
        }

        public async Task<ResidentDto?> GetResidentByApartmentAsync(string apartmentNumber)
        {
            var resident = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .FirstOrDefaultAsync(r => r.ApartmentNumber == apartmentNumber && r.IsActive);

            return resident != null ? MapToDto(resident) : null;
        }

        public async Task<List<ResidentContactDto>> GetPriorityContactsAsync(int residentId, string contactType)
        {
            var contacts = await _context.ResidentContacts
                .Where(c => c.ResidentId == residentId && c.ContactType == contactType && c.IsActive)
                .OrderBy(c => c.Priority)
                .ToListAsync();

            return contacts.Select(MapContactToDto).ToList();
        }

        public async Task<ResidentContactDto?> GetHighestPriorityContactAsync(int residentId, string contactType)
        {
            var contact = await _context.ResidentContacts
                .Where(c => c.ResidentId == residentId && c.ContactType == contactType && c.IsActive)
                .OrderBy(c => c.Priority)
                .FirstOrDefaultAsync();

            return contact != null ? MapContactToDto(contact) : null;
        }

        public async Task<List<ResidentDto>> BulkCreateResidentsAsync(List<CreateResidentDto> residents)
        {
            var createdResidents = new List<ResidentDto>();

            foreach (var residentDto in residents)
            {
                try
                {
                    var created = await CreateResidentAsync(residentDto);
                    createdResidents.Add(created);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating resident {FullName}", residentDto.FullName);
                }
            }

            return createdResidents;
        }

        public async Task<byte[]> ExportResidentsToExcelAsync()
        {
            // Disable GDI+ completely before any Excel operations on macOS
            if (OperatingSystem.IsMacOS())
            {
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT", "1");
                Environment.SetEnvironmentVariable("System.Drawing.EnableUnixSupport", "true");
                AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
            }
            
            try
            {
                // Set EPPlus license for this operation
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            }
            catch
            {
                // If license setting fails, continue anyway
            }

            var residents = await _context.Residents
                .Include(r => r.Contacts.Where(c => c.IsActive))
                .Include(r => r.Vehicles.Where(v => v.IsActive))
                .Where(r => r.IsActive)
                .OrderBy(r => r.Block)
                .ThenBy(r => r.SubBlock)
                .ThenBy(r => r.DoorNumber)
                .ToListAsync();

            using var package = new ExcelPackage();
            
            // Ana sayfa - Daire Sahipleri
            var worksheet = package.Workbook.Worksheets.Add("Daire Sahipleri");
            
            // Başlık satırı
            worksheet.Cells[1, 1].Value = "Ad Soyad";
            worksheet.Cells[1, 2].Value = "Daire No";
            worksheet.Cells[1, 3].Value = "Ana Blok";
            worksheet.Cells[1, 4].Value = "Alt Blok";
            worksheet.Cells[1, 5].Value = "Kapı No";
            worksheet.Cells[1, 6].Value = "Telefon Numaraları";
            worksheet.Cells[1, 7].Value = "E-posta Adresleri";
            worksheet.Cells[1, 8].Value = "Araç Plakaları";
            worksheet.Cells[1, 9].Value = "Notlar";
            worksheet.Cells[1, 10].Value = "Kayıt Tarihi";
            
            // Başlık stilini ayarla (macOS'ta devre dışı)
            if (!OperatingSystem.IsMacOS())
            {
                try
                {
                    using (var range = worksheet.Cells[1, 1, 1, 10])
                    {
                        range.Style.Font.Bold = true;
                    }
                }
                catch
                {
                    // Skip styling if it fails
                }
            }
            
            // Veri satırları
            for (int i = 0; i < residents.Count; i++)
            {
                var resident = residents[i];
                var row = i + 2;
                
                worksheet.Cells[row, 1].Value = resident.FullName;
                worksheet.Cells[row, 2].Value = resident.ApartmentNumber;
                worksheet.Cells[row, 3].Value = resident.Block;
                worksheet.Cells[row, 4].Value = resident.SubBlock;
                worksheet.Cells[row, 5].Value = resident.DoorNumber;
                
                // Telefon numaralarını birleştir
                var phones = resident.Contacts.Where(c => c.ContactType == "Phone").Select(c => c.ContactValue);
                worksheet.Cells[row, 6].Value = string.Join(", ", phones);
                
                // E-posta adreslerini birleştir
                var emails = resident.Contacts.Where(c => c.ContactType == "Email").Select(c => c.ContactValue);
                worksheet.Cells[row, 7].Value = string.Join(", ", emails);
                
                // Araç plakalarını birleştir
                var plates = resident.Vehicles.Select(v => v.LicensePlate);
                worksheet.Cells[row, 8].Value = string.Join(", ", plates);
                
                worksheet.Cells[row, 9].Value = resident.Notes;
                worksheet.Cells[row, 10].Value = resident.CreatedAt.ToString("dd.MM.yyyy");
            }
            
            // Sütun genişliklerini ayarla (macOS'ta devre dışı)
            if (!OperatingSystem.IsMacOS())
            {
                try
                {
                    worksheet.Column(1).Width = 20; // Ad Soyad
                    worksheet.Column(2).Width = 12; // Daire No
                    worksheet.Column(3).Width = 10; // Ana Blok
                    worksheet.Column(4).Width = 10; // Alt Blok
                    worksheet.Column(5).Width = 10; // Kapı No
                    worksheet.Column(6).Width = 25; // Telefon
                    worksheet.Column(7).Width = 30; // E-posta
                    worksheet.Column(8).Width = 20; // Plakalar
                    worksheet.Column(9).Width = 30; // Notlar
                    worksheet.Column(10).Width = 15; // Kayıt Tarihi
                }
                catch
                {
                    // Skip column width settings if they fail
                }
            }
            
            // Detaylı İletişim sayfası
            var contactsSheet = package.Workbook.Worksheets.Add("İletişim Detayları");
            
            // İletişim başlıkları
            contactsSheet.Cells[1, 1].Value = "Ad Soyad";
            contactsSheet.Cells[1, 2].Value = "Daire No";
            contactsSheet.Cells[1, 3].Value = "İletişim Türü";
            contactsSheet.Cells[1, 4].Value = "İletişim Bilgisi";
            contactsSheet.Cells[1, 5].Value = "Etiket";
            contactsSheet.Cells[1, 6].Value = "Öncelik";
            
            // İletişim başlık stilini ayarla (macOS'ta devre dışı)
            if (!OperatingSystem.IsMacOS())
            {
                try
                {
                    using (var range = contactsSheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                    }
                }
                catch
                {
                    // Skip styling if it fails
                }
            }
            
            // İletişim verilerini ekle
            int contactRow = 2;
            foreach (var resident in residents)
            {
                foreach (var contact in resident.Contacts)
                {
                    contactsSheet.Cells[contactRow, 1].Value = resident.FullName;
                    contactsSheet.Cells[contactRow, 2].Value = resident.ApartmentNumber;
                    contactsSheet.Cells[contactRow, 3].Value = contact.ContactType == "Phone" ? "Telefon" : "E-posta";
                    contactsSheet.Cells[contactRow, 4].Value = contact.ContactValue;
                    contactsSheet.Cells[contactRow, 5].Value = contact.Label;
                    contactsSheet.Cells[contactRow, 6].Value = contact.Priority == 1 ? "Yüksek" : 
                                                                contact.Priority == 2 ? "Orta" : "Düşük";
                    contactRow++;
                }
            }
            
            // Araç Detayları sayfası
            var vehiclesSheet = package.Workbook.Worksheets.Add("Araç Detayları");
            
            // Araç başlıkları
            vehiclesSheet.Cells[1, 1].Value = "Ad Soyad";
            vehiclesSheet.Cells[1, 2].Value = "Daire No";
            vehiclesSheet.Cells[1, 3].Value = "Plaka";
            vehiclesSheet.Cells[1, 4].Value = "Marka";
            vehiclesSheet.Cells[1, 5].Value = "Model";
            vehiclesSheet.Cells[1, 6].Value = "Renk";
            vehiclesSheet.Cells[1, 7].Value = "Yıl";
            vehiclesSheet.Cells[1, 8].Value = "Araç Türü";
            vehiclesSheet.Cells[1, 9].Value = "Notlar";
            
            // Araç başlık stilini ayarla (macOS'ta devre dışı)
            if (!OperatingSystem.IsMacOS())
            {
                try
                {
                    using (var range = vehiclesSheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                    }
                }
                catch
                {
                    // Skip styling if it fails
                }
            }
            
            // Araç verilerini ekle
            int vehicleRow = 2;
            foreach (var resident in residents)
            {
                foreach (var vehicle in resident.Vehicles)
                {
                    vehiclesSheet.Cells[vehicleRow, 1].Value = resident.FullName;
                    vehiclesSheet.Cells[vehicleRow, 2].Value = resident.ApartmentNumber;
                    vehiclesSheet.Cells[vehicleRow, 3].Value = vehicle.LicensePlate;
                    vehiclesSheet.Cells[vehicleRow, 4].Value = vehicle.Brand;
                    vehiclesSheet.Cells[vehicleRow, 5].Value = vehicle.Model;
                    vehiclesSheet.Cells[vehicleRow, 6].Value = vehicle.Color;
                    vehiclesSheet.Cells[vehicleRow, 7].Value = vehicle.Year;
                    vehiclesSheet.Cells[vehicleRow, 8].Value = vehicle.VehicleType;
                    vehiclesSheet.Cells[vehicleRow, 9].Value = vehicle.Notes;
                    vehicleRow++;
                }
            }
            
            // Manual column widths are already set above
            
            return package.GetAsByteArray();
        }

        public async Task<List<string>> ImportResidentsFromExcelAsync(byte[] excelData)
        {
            
            var errors = new List<string>();
            var successCount = 0;
            var processedApartments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                using var package = new ExcelPackage(new MemoryStream(excelData));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                
                if (worksheet == null)
                {
                    errors.Add("Excel dosyasında sayfa bulunamadı");
                    return errors;
                }
                
                // Başlık satırını kontrol et
                var headers = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        headers[headerValue] = col;
                    }
                }
                
                // Gerekli sütunları kontrol et
                var requiredColumns = new[] { "Ad Soyad", "Daire No" };
                var missingColumns = requiredColumns.Where(col => !headers.ContainsKey(col)).ToList();
                
                if (missingColumns.Any())
                {
                    errors.Add($"Eksik sütunlar: {string.Join(", ", missingColumns)}");
                    return errors;
                }
                
                // Veri satırlarını işle
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var fullName = worksheet.Cells[row, headers["Ad Soyad"]].Value?.ToString();
                        var apartmentNumber = worksheet.Cells[row, headers["Daire No"]].Value?.ToString();
                        
                        // Zorunlu alanları kontrol et
                        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(apartmentNumber))
                        {
                            errors.Add($"Satır {row}: Ad Soyad ve Daire No zorunludur");
                            continue;
                        }
                        
                        // Excel dosyası içinde duplicate kontrol
                        if (processedApartments.Contains(apartmentNumber))
                        {
                            errors.Add($"Satır {row}: {apartmentNumber} daire numarası Excel dosyasında birden fazla kez kullanılmış");
                            continue;
                        }
                        
                        // Veritabanında daire numarası benzersizlik kontrolü (tüm kayıtlarda, aktif/pasif tümü)
                        var existingResident = await _context.Residents
                            .FirstOrDefaultAsync(r => r.ApartmentNumber.ToLower() == apartmentNumber.ToLower());
                        
                        if (existingResident != null)
                        {
                            if (existingResident.IsActive)
                            {
                                errors.Add($"Satır {row}: {apartmentNumber} daire numarası zaten aktif kayıt olarak mevcut - {existingResident.FullName}");
                            }
                            else
                            {
                                errors.Add($"Satır {row}: {apartmentNumber} daire numarası daha önce kullanılmış (pasif kayıt) - {existingResident.FullName}");
                            }
                            continue;
                        }
                        
                        // İşlenen daire numaralarını takip et
                        processedApartments.Add(apartmentNumber);
                        
                        // Blok bilgilerini al
                        var block = headers.ContainsKey("Ana Blok") ? 
                            worksheet.Cells[row, headers["Ana Blok"]].Value?.ToString()?.Trim() : null;
                        var subBlock = headers.ContainsKey("Alt Blok") ? 
                            worksheet.Cells[row, headers["Alt Blok"]].Value?.ToString()?.Trim() : null;
                        var doorNumber = headers.ContainsKey("Kapı No") ? 
                            worksheet.Cells[row, headers["Kapı No"]].Value?.ToString()?.Trim() : null;
                        
                        // Eğer Excel'de daire numarası yoksa otomatik oluştur
                        if (string.IsNullOrWhiteSpace(apartmentNumber) && !string.IsNullOrWhiteSpace(block) && 
                            !string.IsNullOrWhiteSpace(subBlock) && !string.IsNullOrWhiteSpace(doorNumber))
                        {
                            apartmentNumber = $"{block}{subBlock}-{doorNumber}";
                        }
                        
                        // Yeni resident oluştur
                        var resident = new Resident
                        {
                            FullName = fullName.Trim(),
                            ApartmentNumber = apartmentNumber.Trim(),
                            Block = block,
                            SubBlock = subBlock,
                            DoorNumber = doorNumber,
                            Notes = headers.ContainsKey("Notlar") ? 
                                worksheet.Cells[row, headers["Notlar"]].Value?.ToString()?.Trim() : null,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        
                        _context.Residents.Add(resident);
                        await _context.SaveChangesAsync();
                        
                        // Telefon numaralarını işle
                        if (headers.ContainsKey("Telefon Numaraları"))
                        {
                            var phones = worksheet.Cells[row, headers["Telefon Numaraları"]].Value?.ToString();
                            if (!string.IsNullOrWhiteSpace(phones))
                            {
                                var phoneNumbers = phones.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                var priority = 1;
                                
                                foreach (var phone in phoneNumbers)
                                {
                                    var contact = new ResidentContact
                                    {
                                        ResidentId = resident.Id,
                                        ContactType = "Phone",
                                        ContactValue = phone.Trim(),
                                        Priority = priority++,
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    _context.ResidentContacts.Add(contact);
                                }
                            }
                        }
                        
                        // E-posta adreslerini işle
                        if (headers.ContainsKey("E-posta Adresleri"))
                        {
                            var emails = worksheet.Cells[row, headers["E-posta Adresleri"]].Value?.ToString();
                            if (!string.IsNullOrWhiteSpace(emails))
                            {
                                var emailAddresses = emails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                var priority = 1;
                                
                                foreach (var email in emailAddresses)
                                {
                                    if (IsValidEmail(email.Trim()))
                                    {
                                        var contact = new ResidentContact
                                        {
                                            ResidentId = resident.Id,
                                            ContactType = "Email",
                                            ContactValue = email.Trim(),
                                            Priority = priority++,
                                            CreatedAt = DateTime.UtcNow,
                                            IsActive = true
                                        };
                                        _context.ResidentContacts.Add(contact);
                                    }
                                }
                            }
                        }
                        
                        // Araç plakalarını işle
                        if (headers.ContainsKey("Araç Plakaları"))
                        {
                            var plates = worksheet.Cells[row, headers["Araç Plakaları"]].Value?.ToString();
                            if (!string.IsNullOrWhiteSpace(plates))
                            {
                                var licensePlates = plates.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                
                                foreach (var plate in licensePlates)
                                {
                                    var vehicle = new ResidentVehicle
                                    {
                                        ResidentId = resident.Id,
                                        LicensePlate = plate.Trim(),
                                        VehicleType = "Car",
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    _context.ResidentVehicles.Add(vehicle);
                                }
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Satır {row}: {ex.Message}");
                        _logger.LogError(ex, "Error importing resident at row {Row}", row);
                    }
                }
                
                if (successCount > 0)
                {
                    errors.Insert(0, $"Başarılı: {successCount} kayıt eklendi");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Excel dosyası işlenirken hata: {ex.Message}");
                _logger.LogError(ex, "Error processing Excel file");
            }
            
            return errors;
        }
        
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Private mapping methods
        private ResidentDto MapToDto(Resident resident)
        {
            // Create block display as "A3" format (Block + SubBlock) for table display
            var blockDisplay = !string.IsNullOrEmpty(resident.Block) && !string.IsNullOrEmpty(resident.SubBlock)
                ? $"{resident.Block}{resident.SubBlock}"
                : resident.Block ?? "";

            return new ResidentDto
            {
                Id = resident.Id,
                FullName = resident.FullName,
                ApartmentNumber = resident.ApartmentNumber,
                Block = blockDisplay, // This will show as "A3" for table display
                SubBlock = resident.SubBlock, // Keep original SubBlock for form editing
                DoorNumber = resident.DoorNumber,
                Notes = resident.Notes,
                IsActive = resident.IsActive,
                CreatedAt = resident.CreatedAt,
                UpdatedAt = resident.UpdatedAt,
                Contacts = resident.Contacts.Select(MapContactToDto).ToList(),
                Vehicles = resident.Vehicles.Select(MapVehicleToDto).ToList(),
                // Add original block and subblock for form editing
                OriginalBlock = resident.Block,
                OriginalSubBlock = resident.SubBlock
            };
        }

        private ResidentContactDto MapContactToDto(ResidentContact contact)
        {
            return new ResidentContactDto
            {
                Id = contact.Id,
                ResidentId = contact.ResidentId,
                ContactType = contact.ContactType,
                ContactValue = contact.ContactValue,
                Label = contact.Label,
                Priority = contact.Priority,
                IsActive = contact.IsActive,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };
        }

        private ResidentVehicleDto MapVehicleToDto(ResidentVehicle vehicle)
        {
            return new ResidentVehicleDto
            {
                Id = vehicle.Id,
                ResidentId = vehicle.ResidentId,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Color = vehicle.Color,
                Year = vehicle.Year,
                VehicleType = vehicle.VehicleType,
                Notes = vehicle.Notes,
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        // Turkish character normalization method
        private string NormalizeTurkishText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u")
                .Replace("Ç", "c")
                .Replace("Ğ", "g")
                .Replace("İ", "i")
                .Replace("Ö", "o")
                .Replace("Ş", "s")
                .Replace("Ü", "u");
        }
    }
}