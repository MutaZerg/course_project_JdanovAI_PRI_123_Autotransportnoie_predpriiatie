using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleCompany.Attributes;
using VehicleCompany.Services;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthService authService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [Permission("users.view")]
        public IActionResult Users()
        {
            return View();
        }

        [Permission("users.edit")]
        public IActionResult EditUser(int id)
        {
            return View();
        }

        [Permission("settings.view")]
        public IActionResult Settings()
        {
            return View();
        }

        [Authorize(Roles = "Admin")] // Также можно проверять по ролям
        public IActionResult AdminPanel()
        {
            return View();
        }
    }
}