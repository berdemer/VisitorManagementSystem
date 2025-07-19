using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services
{
    public class NotificationLogService : INotificationLogService
    {
        private readonly ApplicationDbContext _context;

        public NotificationLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogNotificationAsync(NotificationLog log)
        {
            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogSmsAsync(string apartmentNumber, string? visitorName, string? visitorPhone, 
                                     string recipientPhone, string subject, string content, string status, 
                                     string? errorMessage = null, string? externalId = null, string? sentBy = null,
                                     int? visitorId = null, int? residentId = null)
        {
            var log = new NotificationLog
            {
                NotificationType = "SMS",
                ApartmentNumber = apartmentNumber,
                VisitorName = visitorName,
                VisitorPhone = visitorPhone,
                RecipientPhone = recipientPhone,
                Subject = subject,
                Content = content,
                Status = status,
                ErrorMessage = errorMessage,
                ExternalId = externalId,
                SentBy = sentBy,
                SentAt = DateTime.UtcNow,
                VisitorId = visitorId,
                ResidentId = residentId,
                IsActive = true
            };

            await LogNotificationAsync(log);
        }

        public async Task LogEmailAsync(string apartmentNumber, string? visitorName, string? visitorPhone, 
                                       string recipientEmail, string subject, string content, string status, 
                                       string? errorMessage = null, string? externalId = null, string? sentBy = null,
                                       int? visitorId = null, int? residentId = null)
        {
            var log = new NotificationLog
            {
                NotificationType = "EMAIL",
                ApartmentNumber = apartmentNumber,
                VisitorName = visitorName,
                VisitorPhone = visitorPhone,
                RecipientEmail = recipientEmail,
                Subject = subject,
                Content = content,
                Status = status,
                ErrorMessage = errorMessage,
                ExternalId = externalId,
                SentBy = sentBy,
                SentAt = DateTime.UtcNow,
                VisitorId = visitorId,
                ResidentId = residentId,
                IsActive = true
            };

            await LogNotificationAsync(log);
        }

        public async Task<IEnumerable<NotificationLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? type = null, 
                                                                    string? apartmentNumber = null, DateTime? fromDate = null, 
                                                                    DateTime? toDate = null)
        {
            var query = _context.NotificationLogs
                .Include(l => l.Visitor)
                .Include(l => l.Resident)
                .Where(l => l.IsActive);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(l => l.NotificationType == type.ToUpper());

            if (!string.IsNullOrEmpty(apartmentNumber))
                query = query.Where(l => l.ApartmentNumber == apartmentNumber);

            if (fromDate.HasValue)
                query = query.Where(l => l.SentAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.SentAt <= toDate.Value.AddDays(1));

            return await query
                .OrderByDescending(l => l.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLogCountAsync(string? type = null, string? apartmentNumber = null, 
                                               DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.NotificationLogs.Where(l => l.IsActive);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(l => l.NotificationType == type.ToUpper());

            if (!string.IsNullOrEmpty(apartmentNumber))
                query = query.Where(l => l.ApartmentNumber == apartmentNumber);

            if (fromDate.HasValue)
                query = query.Where(l => l.SentAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.SentAt <= toDate.Value.AddDays(1));

            return await query.CountAsync();
        }

        public async Task<NotificationLog?> GetLogByIdAsync(int id)
        {
            return await _context.NotificationLogs
                .Include(l => l.Visitor)
                .Include(l => l.Resident)
                .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);
        }

        public async Task UpdateLogStatusAsync(int id, string status, string? errorMessage = null, string? externalId = null)
        {
            var log = await _context.NotificationLogs.FindAsync(id);
            if (log != null)
            {
                log.Status = status;
                if (!string.IsNullOrEmpty(errorMessage))
                    log.ErrorMessage = errorMessage;
                if (!string.IsNullOrEmpty(externalId))
                    log.ExternalId = externalId;

                await _context.SaveChangesAsync();
            }
        }
    }
}