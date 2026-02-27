using Microsoft.EntityFrameworkCore;
using VehicleCompany.Contexts;
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

            var storedPassword = user.Password ?? string.Empty;

            // Valid BCrypt hashes start with $2a$, $2b$, or $2y$
            if (storedPassword.StartsWith("$2a$", StringComparison.Ordinal) ||
                storedPassword.StartsWith("$2b$", StringComparison.Ordinal) ||
                storedPassword.StartsWith("$2y$", StringComparison.Ordinal))
            {
                try
                {
                    if (!BCrypt.Net.BCrypt.Verify(password, storedPassword))
                        return null;
                }
                catch (Exception ex)
                {
                    // Invalid salt/hash format (e.g. different BCrypt library or corrupted data)
                    _logger.LogWarning(ex, "BCrypt verify failed for user {UserName}. Stored hash may be invalid or from another library.", username);
                    return null;
                }
            }
            else
            {
                // Not a BCrypt hash (e.g. plain text or other format) — compare as plain text for backward compatibility
                if (password != storedPassword)
                    return null;
                // Optionally re-hash with BCrypt and save so next login uses BCrypt
                try
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not upgrade password to BCrypt for user {UserName}. Login still allowed.", username);
                }
            }

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

        public async Task<RegisterResult> RegisterAsync(RegisterViewModel model)
        {
            var result = new RegisterResult();

            try
            {
                
                // Проверка существования пользователя
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName);

                if (existingUser != null)
                {
                    _logger.LogInformation("Пользователь с таким именем уже существует", "as");
                    result.Errors.Add("Пользователь с таким именем уже существует");
                    return result;
                }

                // Проверка email (если используете)
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingEmail != null)
                {
                    _logger.LogInformation("Пользователь с таким email уже существует", "as");
                    result.Errors.Add("Пользователь с таким email уже существует");
                    return result;
                }

                // Создание нового пользователя
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email, // Заглушка
                    Password = model.Password
                };
                _logger.LogInformation("Пользователь cjplf", "as");

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Назначение роли по умолчанию (например, "User")
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == "User");

                if (defaultRole != null)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id
                    });
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Новый пользователь зарегистрирован: {UserName}", user.UserName);

                result.Success = true;
                result.Message = "Регистрация прошла успешно";
                result.User = user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя");
                result.Errors.Add("Произошла ошибка при регистрации");
            }

            return result;
        }

        public async Task<bool> ValidateUserNameAsync(string userName)
        {
            return !await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

    }
}