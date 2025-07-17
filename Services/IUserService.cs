using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}