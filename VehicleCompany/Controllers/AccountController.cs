using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleCompany.Controllers;
using VehicleCompany.Services;
using VehicleCompany.Models;

namespace VehicleCompany.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Accounts/Login.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticateAsync(model.UserName, model.Password);

                if (user != null)
                {
                    var userSession = await _authService.GetUserSessionAsync(user.Id);

                    if (userSession != null)
                    {
                        // Create claims identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email)
                        };

                        // Add role claims
                        foreach (var role in userSession.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        // Add permission claims
                        foreach (var permission in userSession.Permissions)
                        {
                            claims.Add(new Claim("Permission", permission));
                        }

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = model.RememberMe ?
                                DateTimeOffset.UtcNow.AddDays(30) :
                                DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // Store user info in session (optional)
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("UserName", user.UserName);

                        _logger.LogInformation("User {UserName} logged in.", user.UserName);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
            }

            return View("~/Views/Accounts/Login.cshtml", model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    HttpContext.Session.Clear();
        //    _logger.LogInformation("User logged out.");
        //    return RedirectToAction(nameof(HomeController.Index), "Home");
        //}

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var userSession = await _authService.GetUserSessionAsync(long.Parse(userId));

            if (userSession == null)
            {
                return NotFound();
            }

            return View("~/Views/Accounts/Profile.cshtml", userSession);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Accounts/AccessDenied.cshtml");
        }
    }
}