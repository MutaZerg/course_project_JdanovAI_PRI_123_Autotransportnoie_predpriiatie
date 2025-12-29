using System.ComponentModel.DataAnnotations;

namespace VehicleCompany.Models
{
    public class Trip
    {
        public required long Id { get; set; }
        public required long Route_id { get; set; }
        [DataType(DataType.DateTime)]
        public required System.DateTime Start_time { get; set; }
        [DataType(DataType.DateTime)]
        public required System.DateTime End_time { get; set; }
        public required long Assigned_vehicle { get; set; }
    }
}
