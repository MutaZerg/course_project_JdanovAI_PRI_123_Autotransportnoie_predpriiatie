using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("Route_stop")]
    public class RouteStop
    {
        [Column("route_id")]
        public long RouteId { get; set; }

        [Column("stop_id")]
        public long StopId { get; set; }

        [ForeignKey("RouteId")]
        public virtual Route? Route { get; set; }

        [ForeignKey("StopId")]
        public virtual Stop? Stop { get; set; }
    }
}
