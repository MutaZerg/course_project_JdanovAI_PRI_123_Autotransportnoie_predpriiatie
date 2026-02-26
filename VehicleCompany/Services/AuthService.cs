using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using VehicleCompany.Services;
//using VehicleCompany.Contexts;
using VehicleCompany.Models;

namespace VehicleCompany.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return null;

            // Verify password using BCrypt
            if (!BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }

        public async Task<UserSession?> GetUserSessionAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var session = new UserSession
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = new List<string>(),
                Permissions = new List<string>()
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    session.Roles.Add(userRole.Role.RoleName);

                    foreach (var rolePermission in userRole.Role.RolePermissions)
                    {
                        if (rolePermission.Permission != null &&
                            !session.Permissions.Contains(rolePermission.Permission.Action))
                        {
                            session.Permissions.Add(rolePermission.Permission.Action);
                        }
                    }
                }
            }

            return session;
        }

        public async Task<bool> HasPermissionAsync(long userId, string permission)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permission);
        }

        public async Task<List<string>> GetUserPermissionsAsync(long userId)
        {
            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Action)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<List<string>> GetUserRolesAsync(long userId)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            return roles;
        }
    }
}