using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using VehicleCompany.Services;

namespace VehicleCompany.Attributes
{

    public class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;
        
        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();

            if (authService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var hasPermission = authService.HasPermissionAsync(long.Parse(userId), _permission).Result;

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}