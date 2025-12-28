namespace VehicleCompany.Models
{
    public class User
    {
        public long Id { get; set; }
        public required string User_name { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}
