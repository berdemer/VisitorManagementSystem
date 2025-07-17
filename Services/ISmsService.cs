namespace VisitorManagementSystem.Services
{
    public interface ISmsService
    {
        Task<bool> SendVisitorNotificationAsync(string phoneNumber, string visitorName, string apartmentNumber);
        Task<bool> SendAlertAsync(string phoneNumber, string message);
    }
}