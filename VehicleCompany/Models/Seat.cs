using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("Seat")]
    public class Seat
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("assigned_vehicle")]
        public long AssignedVehicleId { get; set; }

        [Column("is_booked")]
        public bool IsBooked { get; set; }

        [ForeignKey("AssignedVehicleId")]
        public virtual Vehicle? AssignedVehicle { get; set; }
    }
}
