namespace VehicleCompany.Models
{
    public class Trip
    {
        public required long Id { get; set; }
        public required long Route_id { get; set; }
        public required System.DateTime Start_time { get; set; }
        public required System.DateTime End_time { get; set; }
        public required long Assigned_vehicle { get; set; }
    }
}
