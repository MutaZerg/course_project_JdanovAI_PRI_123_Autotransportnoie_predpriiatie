namespace VehicleCompany.Models
{
    public class Booking
    {
        public required long Id { get; set; }
        public required long User_id { get; set; }
        public required long Seat_id { get; set; }
        public required long Trip_id { get; set; }
    }
}
