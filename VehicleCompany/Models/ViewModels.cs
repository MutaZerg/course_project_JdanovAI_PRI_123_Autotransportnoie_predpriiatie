using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
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

    /// <summary>View model for Book Trip page: route with its trips and available seats per trip.</summary>
    public class BookTripViewModel
    {
        public Route Route { get; set; } = null!;
        public List<TripSeatsViewModel> TripsWithSeats { get; set; } = new List<TripSeatsViewModel>();
    }

    public class TripSeatsViewModel
    {
        public Trip Trip { get; set; } = null!;
        public List<Seat> AvailableSeats { get; set; } = new List<Seat>();
    }

    public class RoleSelectionViewModel
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class EditUserRolesViewModel
    {
        [Key]
        public long Id { get; set; }

        [Column("User_name")]
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Column("Password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("Email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty; //заглушка
        public long SelectedRoleId { get; set; }

        public List<RoleSelectionViewModel> Roles { get; set; } = new List<RoleSelectionViewModel>();
    }
}