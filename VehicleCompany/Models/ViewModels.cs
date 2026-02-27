using System.ComponentModel.DataAnnotations;

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
}