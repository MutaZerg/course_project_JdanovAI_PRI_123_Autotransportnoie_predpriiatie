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


    public class TripCreateViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Выберите маршрут")]
        [Display(Name = "Маршрут")]
        public long RouteId { get; set; }

        [Required(ErrorMessage = "Укажите время отправления")]
        [Display(Name = "Время отправления")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Укажите время прибытия")]
        [Display(Name = "Время прибытия")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(2);

        [Display(Name = "Транспортное средство")]
        public long AssignedVehicleId { get; set; }

        // Display properties
        public string? RouteInfo { get; set; }
        public string? RouteStops { get; set; }

        // List of available vehicles (only unassigned ones)
        public List<AvailableVehicleViewModel> AvailableVehicles { get; set; } = new();
    }

    public class AvailableVehicleViewModel
    {
        public long VehicleId { get; set; }
        public string VehicleInfo { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsAssigned { get; set; } // New property to indicate if vehicle is already assigned
    }

    public class UserBookingViewModel
    {
        public long BookingId { get; set; }
        public long TripId { get; set; }
        public string TripName { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string RouteInfo { get; set; } = string.Empty;
        public string VehicleInfo { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}