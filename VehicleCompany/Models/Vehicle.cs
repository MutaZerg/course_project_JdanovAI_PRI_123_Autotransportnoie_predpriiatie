using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("Vehicle")]
    public class Vehicle
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("vehicle_info")]
        public string Vehicle_info { get; set; } = string.Empty;

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
