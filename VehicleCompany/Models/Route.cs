using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace VehicleCompany.Models
{
    public class Route
    {
        public required long Id { get; set; }
        [DataType(DataType.Time)]
        public required System.TimeSpan Travel_time { get; set; }
        public required long Price { get; set; }

        public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();

    }
}
