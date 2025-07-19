using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services
{
    public interface INotificationLogService
    {
        Task LogNotificationAsync(NotificationLog log);
        Task LogSmsAsync(string apartmentNumber, string? visitorName, string? visitorPhone, 
                        string recipientPhone, string subject, string content, string status, 
                        string? errorMessage = null, string? externalId = null, string? sentBy = null,
                        int? visitorId = null, int? residentId = null);
        Task LogEmailAsync(string apartmentNumber, string? visitorName, string? visitorPhone, 
                          string recipientEmail, string subject, string content, string status, 
                          string? errorMessage = null, string? externalId = null, string? sentBy = null,
                          int? visitorId = null, int? residentId = null);
        Task<IEnumerable<NotificationLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? type = null, 
                                                       string? apartmentNumber = null, DateTime? fromDate = null, 
                                                       DateTime? toDate = null);
        Task<int> GetLogCountAsync(string? type = null, string? apartmentNumber = null, 
                                  DateTime? fromDate = null, DateTime? toDate = null);
        Task<NotificationLog?> GetLogByIdAsync(int id);
        Task UpdateLogStatusAsync(int id, string status, string? errorMessage = null, string? externalId = null);
    }
}