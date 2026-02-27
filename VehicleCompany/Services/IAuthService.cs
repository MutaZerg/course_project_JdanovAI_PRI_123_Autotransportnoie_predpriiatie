using VehicleCompany.Models;

namespace VehicleCompany.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<UserSession?> GetUserSessionAsync(long userId);
        Task<bool> HasPermissionAsync(long userId, string permission);
        Task<List<string>> GetUserPermissionsAsync(long userId);
        Task<List<string>> GetUserRolesAsync(long userId);
        Task<RegisterResult> RegisterAsync(RegisterViewModel model);
        Task<bool> ValidateUserNameAsync(string userName);
        Task<bool> ValidateEmailAsync(string email);
    }
}