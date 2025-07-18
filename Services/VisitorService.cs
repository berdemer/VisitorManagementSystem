using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace VisitorManagementSystem.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly ApplicationDbContext _context;

        public VisitorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Visitor>> GetAllVisitorsAsync()
        {
            return await _context.Visitors
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Visitor>> GetActiveVisitorsAsync()
        {
            return await _context.Visitors
                .Where(v => v.IsActive && v.CheckOutTime == null)
                .OrderBy(v => v.CheckInTime)
                .ToListAsync();
        }

        public async Task<Visitor?> GetVisitorByIdAsync(int id)
        {
            return await _context.Visitors.FindAsync(id);
        }

        public async Task<Visitor> CreateVisitorAsync(Visitor visitor)
        {
            visitor.CheckInTime = DateTime.Now;
            visitor.CreatedAt = DateTime.Now;
            visitor.IsActive = true;

            _context.Visitors.Add(visitor);
            await _context.SaveChangesAsync();

            // Log the check-in
            var log = new VisitorLog
            {
                VisitorId = visitor.Id,
                Action = "CheckIn",
                Timestamp = DateTime.Now,
                Details = $"Visitor {visitor.FullName} checked in to apartment {visitor.ApartmentNumber}",
                PerformedBy = visitor.CreatedBy
            };
            _context.VisitorLogs.Add(log);
            await _context.SaveChangesAsync();

            return visitor;
        }

        public async Task<Visitor> UpdateVisitorAsync(Visitor visitor)
        {
            var existingVisitor = await _context.Visitors.FindAsync(visitor.Id);
            if (existingVisitor == null)
                throw new InvalidOperationException("Visitor not found");

            // Preserve original timestamps and system fields
            var originalCheckInTime = existingVisitor.CheckInTime;
            var originalCreatedAt = existingVisitor.CreatedAt;
            var originalCreatedBy = existingVisitor.CreatedBy;
            var originalCheckOutTime = existingVisitor.CheckOutTime;
            var originalIsActive = existingVisitor.IsActive;
            var originalPhotoPath = existingVisitor.PhotoPath;

            // Update only the editable fields
            existingVisitor.FullName = visitor.FullName;
            existingVisitor.ApartmentNumber = visitor.ApartmentNumber;
            existingVisitor.ResidentName = visitor.ResidentName;
            existingVisitor.ResidentPhone = visitor.ResidentPhone;
            existingVisitor.VisitorPhone = visitor.VisitorPhone;
            existingVisitor.LicensePlate = visitor.LicensePlate;
            existingVisitor.Notes = visitor.Notes;
            
            // Restore preserved fields
            existingVisitor.CheckInTime = originalCheckInTime;
            existingVisitor.CreatedAt = originalCreatedAt;
            existingVisitor.CreatedBy = originalCreatedBy;
            existingVisitor.CheckOutTime = originalCheckOutTime;
            existingVisitor.IsActive = originalIsActive;
            
            // Update photoPath only if a new one is provided, otherwise preserve existing
            if (!string.IsNullOrEmpty(visitor.PhotoPath))
            {
                existingVisitor.PhotoPath = visitor.PhotoPath;
            }
            else
            {
                existingVisitor.PhotoPath = originalPhotoPath;
            }

            await _context.SaveChangesAsync();
            return existingVisitor;
        }

        public async Task<bool> CheckOutVisitorAsync(int id, string performedBy)
        {
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor == null || visitor.CheckOutTime.HasValue)
                return false;

            visitor.CheckOutTime = DateTime.Now;
            visitor.IsActive = false;

            // Log the check-out
            var log = new VisitorLog
            {
                VisitorId = visitor.Id,
                Action = "CheckOut",
                Timestamp = DateTime.Now,
                Details = $"Visitor {visitor.FullName} checked out from apartment {visitor.ApartmentNumber}",
                PerformedBy = performedBy
            };
            _context.VisitorLogs.Add(log);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Visitor>> GetVisitorsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var endDateWithTime = endDate.AddDays(1);
            
            return await _context.Visitors
                .Where(v => 
                    // Check-in in date range
                    (v.CheckInTime >= startDate && v.CheckInTime < endDateWithTime) ||
                    // OR still active (no checkout) - include all active visitors regardless of check-in date
                    (v.CheckOutTime == null && v.IsActive) ||
                    // OR checked out in date range
                    (v.CheckOutTime.HasValue && v.CheckOutTime >= startDate && v.CheckOutTime < endDateWithTime)
                )
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Visitor>> GetVisitorsByApartmentAsync(string apartmentNumber)
        {
            return await _context.Visitors
                .Where(v => v.ApartmentNumber == apartmentNumber)
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();
        }

        public async Task<bool> DeleteVisitorAsync(int id)
        {
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor == null)
                return false;

            _context.Visitors.Remove(visitor);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<byte[]> ExportVisitorsToExcelAsync(IEnumerable<Visitor> visitors, DateTime startDate, DateTime endDate)
        {
            // Disable GDI+ completely before any Excel operations on macOS
            if (OperatingSystem.IsMacOS())
            {
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT", "1");
                Environment.SetEnvironmentVariable("System.Drawing.EnableUnixSupport", "false");
                AppContext.SetSwitch("System.Drawing.EnableUnixSupport", false);
                
                // Additional EPPlus macOS configuration
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT", "1");
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT_LEGACY", "1");
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP", "1");
            }
            
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Ziyaretçiler");

                // Headers
                worksheet.Cells[1, 1].Value = "Ad Soyad";
                worksheet.Cells[1, 2].Value = "Daire No";
                worksheet.Cells[1, 3].Value = "Telefon";
                worksheet.Cells[1, 4].Value = "Plaka";
                worksheet.Cells[1, 5].Value = "Ziyaret Nedeni";
                worksheet.Cells[1, 6].Value = "Giriş Tarihi";
                worksheet.Cells[1, 7].Value = "Çıkış Tarihi";
                worksheet.Cells[1, 8].Value = "Durum";
                worksheet.Cells[1, 9].Value = "Daire Sahibi";

                // Style headers (macOS'ta devre dışı)
                if (!OperatingSystem.IsMacOS())
                {
                    try
                    {
                        using var range = worksheet.Cells[1, 1, 1, 9];
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                    catch
                    {
                        // Skip styling if it fails
                    }
                }

                // Data
                int row = 2;
                var visitorList = visitors.ToList();
                
                foreach (var visitor in visitorList)
                {
                    worksheet.Cells[row, 1].Value = visitor.FullName ?? "";
                    worksheet.Cells[row, 2].Value = visitor.ApartmentNumber ?? "";
                    worksheet.Cells[row, 3].Value = visitor.VisitorPhone ?? "";
                    worksheet.Cells[row, 4].Value = visitor.LicensePlate ?? "";
                    worksheet.Cells[row, 5].Value = visitor.Notes ?? "";
                    worksheet.Cells[row, 6].Value = visitor.CheckInTime.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 7].Value = visitor.CheckOutTime?.ToString("dd.MM.yyyy HH:mm") ?? "";
                    worksheet.Cells[row, 8].Value = visitor.IsActive ? "Aktif" : "Çıkış Yapılmış";
                    worksheet.Cells[row, 9].Value = visitor.ResidentName ?? "";
                    row++;
                }

                // Auto-fit columns (macOS'ta devre dışı)
                if (!OperatingSystem.IsMacOS())
                {
                    try
                    {
                        worksheet.Cells.AutoFitColumns();
                    }
                    catch
                    {
                        // Skip auto-fit if it fails
                    }
                }

                // Add summary
                worksheet.Cells[row + 1, 1].Value = "Rapor Özeti";
                
                // Style summary (macOS'ta devre dışı)
                if (!OperatingSystem.IsMacOS())
                {
                    try
                    {
                        worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    }
                    catch
                    {
                        // Skip styling if it fails
                    }
                }
                worksheet.Cells[row + 2, 1].Value = $"Tarih Aralığı: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                worksheet.Cells[row + 3, 1].Value = $"Toplam Ziyaretçi: {visitorList.Count}";
                worksheet.Cells[row + 4, 1].Value = $"Aktif Ziyaretçi: {visitorList.Count(v => v.IsActive)}";
                worksheet.Cells[row + 5, 1].Value = $"Çıkış Yapılmış: {visitorList.Count(v => !v.IsActive)}";
                worksheet.Cells[row + 6, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";

                var result = package.GetAsByteArray();
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Excel export error: {ex.Message}");
                
                // If it's a GDI+ error on macOS, try a simpler approach
                if (OperatingSystem.IsMacOS() && ex.Message.Contains("Gdip"))
                {
                    return CreateSimpleExcelReport(visitors, startDate, endDate);
                }
                
                throw;
            }
        }

        private Task<byte[]> CreateSimpleExcelReport(IEnumerable<Visitor> visitors, DateTime startDate, DateTime endDate)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Ziyaretçiler");

                // Headers - no styling
                worksheet.Cells[1, 1].Value = "Ad Soyad";
                worksheet.Cells[1, 2].Value = "Daire No";
                worksheet.Cells[1, 3].Value = "Telefon";
                worksheet.Cells[1, 4].Value = "Kimlik No";
                worksheet.Cells[1, 5].Value = "Plaka";
                worksheet.Cells[1, 6].Value = "Giriş Tarihi";
                worksheet.Cells[1, 7].Value = "Çıkış Tarihi";
                worksheet.Cells[1, 8].Value = "Durum";
                worksheet.Cells[1, 9].Value = "Daire Sahibi";
                worksheet.Cells[1, 10].Value = "Notlar";

                // Data only - no styling
                int row = 2;
                var visitorList = visitors.ToList();
                
                foreach (var visitor in visitorList)
                {
                    worksheet.Cells[row, 1].Value = visitor.FullName ?? "";
                    worksheet.Cells[row, 2].Value = visitor.ApartmentNumber ?? "";
                    worksheet.Cells[row, 3].Value = visitor.VisitorPhone ?? "";
                    worksheet.Cells[row, 4].Value = visitor.IdNumber ?? "";
                    worksheet.Cells[row, 5].Value = visitor.LicensePlate ?? "";
                    worksheet.Cells[row, 6].Value = visitor.CheckInTime.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[row, 7].Value = visitor.CheckOutTime?.ToString("dd.MM.yyyy HH:mm") ?? "";
                    worksheet.Cells[row, 8].Value = visitor.IsActive ? "Aktif" : "Çıkış Yapılmış";
                    worksheet.Cells[row, 9].Value = visitor.ResidentName ?? "";
                    worksheet.Cells[row, 10].Value = visitor.Notes ?? "";
                    row++;
                }

                // Add summary - no styling
                worksheet.Cells[row + 1, 1].Value = "Rapor Özeti";
                worksheet.Cells[row + 2, 1].Value = $"Tarih Aralığı: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                worksheet.Cells[row + 3, 1].Value = $"Toplam Ziyaretçi: {visitorList.Count}";
                worksheet.Cells[row + 4, 1].Value = $"Aktif Ziyaretçi: {visitorList.Count(v => v.IsActive)}";
                worksheet.Cells[row + 5, 1].Value = $"Çıkış Yapılmış: {visitorList.Count(v => !v.IsActive)}";
                worksheet.Cells[row + 6, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";

                var result = package.GetAsByteArray();
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simple Excel export error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ApartmentVisitStatDto>> GetMostVisitedApartmentsAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Visitors.AsQueryable();

                Console.WriteLine($"GetMostVisitedApartments called with startDate: {startDate}, endDate: {endDate}");

                if (startDate.HasValue)
                    query = query.Where(v => v.CheckInTime >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(v => v.CheckInTime < endDate.Value.AddDays(1));

                var totalVisitors = await query.CountAsync();
                Console.WriteLine($"Total visitors in query: {totalVisitors}");

                var stats = await query
                    .GroupBy(v => v.ApartmentNumber)
                    .Select(g => new ApartmentVisitStatDto
                    {
                        ApartmentNumber = g.Key,
                        VisitorCount = g.Count(),
                        ActiveVisitorCount = g.Count(v => v.IsActive),
                        LastVisitDate = g.Max(v => v.CheckInTime),
                        MostFrequentVisitor = g.GroupBy(v => v.FullName)
                            .OrderByDescending(vg => vg.Count())
                            .Select(vg => vg.Key)
                            .FirstOrDefault() ?? ""
                    })
                    .OrderByDescending(s => s.VisitorCount)
                    .Take(10)
                    .ToListAsync();

                Console.WriteLine($"Apartment stats result count: {stats.Count}");
                foreach (var stat in stats)
                {
                    Console.WriteLine($"Apartment: {stat.ApartmentNumber}, Visitors: {stat.VisitorCount}");
                }

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMostVisitedApartments: {ex.Message}");
                throw;
            }
        }

        public async Task<PagedVisitorDto> GetVisitorsPagedAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Visitors.AsQueryable();

            var totalCount = await query.CountAsync();

            var visitors = await query
                .OrderByDescending(v => v.CheckInTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedVisitorDto
            {
                Visitors = visitors.Select(MapToVisitorDto).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedVisitorDto> SearchVisitorsAsync(VisitorSearchDto searchDto)
        {
            var query = _context.Visitors.AsQueryable();

            // Date filter
            if (!string.IsNullOrEmpty(searchDto.Date))
            {
                if (DateTime.TryParse(searchDto.Date, out DateTime filterDate))
                {
                    var startDate = filterDate.Date;
                    var endDate = startDate.AddDays(1);
                    query = query.Where(v => v.CheckInTime >= startDate && v.CheckInTime < endDate);
                }
            }

            // Apartment filter
            if (!string.IsNullOrEmpty(searchDto.ApartmentNumber))
            {
                query = query.Where(v => v.ApartmentNumber.Contains(searchDto.ApartmentNumber));
            }

            // License plate filter
            if (!string.IsNullOrEmpty(searchDto.LicensePlate))
            {
                var plateUpper = searchDto.LicensePlate.ToUpper();
                query = query.Where(v => v.LicensePlate != null && v.LicensePlate.ToUpper().Contains(plateUpper));
            }

            // Status filter
            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                if (searchDto.Status == "active")
                {
                    query = query.Where(v => v.IsActive && v.CheckOutTime == null);
                }
                else if (searchDto.Status == "inactive")
                {
                    query = query.Where(v => !v.IsActive || v.CheckOutTime != null);
                }
            }

            var totalCount = await query.CountAsync();

            var visitors = await query
                .OrderByDescending(v => v.CheckInTime)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return new PagedVisitorDto
            {
                Visitors = visitors.Select(MapToVisitorDto).ToList(),
                TotalCount = totalCount,
                PageNumber = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }

        private VisitorDto MapToVisitorDto(Visitor visitor)
        {
            return new VisitorDto
            {
                Id = visitor.Id,
                FullName = visitor.FullName,
                IdNumber = visitor.IdNumber,
                ApartmentNumber = visitor.ApartmentNumber,
                LicensePlate = visitor.LicensePlate,
                CheckInTime = visitor.CheckInTime,
                CheckOutTime = visitor.CheckOutTime,
                IsActive = visitor.IsActive,
                PhotoPath = visitor.PhotoPath,
                Notes = visitor.Notes,
                ResidentName = visitor.ResidentName,
                ResidentPhone = visitor.ResidentPhone,
                VisitorPhone = visitor.VisitorPhone,
                CreatedAt = visitor.CreatedAt,
                CreatedBy = visitor.CreatedBy
            };
        }

        public async Task<IEnumerable<Visitor>> SearchVisitorsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                return new List<Visitor>();

            // Use simpler LIKE search for database compatibility
            var searchTerm = name.ToLowerInvariant();

            return await _context.Visitors
                .Where(v => EF.Functions.Like(v.FullName.ToLower(), $"%{searchTerm}%"))
                .Where(v => !string.IsNullOrEmpty(v.FullName))
                .OrderByDescending(v => v.CheckInTime)
                .Take(20)
                .ToListAsync();
        }

        private static string NormalizeTurkishChars(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace('ç', 'c').Replace('Ç', 'C')
                .Replace('ğ', 'g').Replace('Ğ', 'G')
                .Replace('ı', 'i').Replace('I', 'I')
                .Replace('ö', 'o').Replace('Ö', 'O')
                .Replace('ş', 's').Replace('Ş', 'S')
                .Replace('ü', 'u').Replace('Ü', 'U');
        }
    }
}