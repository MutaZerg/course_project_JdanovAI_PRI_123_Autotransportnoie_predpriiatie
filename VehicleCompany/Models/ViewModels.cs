using System.ComponentModel.DataAnnotations;

namespace YourApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class UserSession
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }
}