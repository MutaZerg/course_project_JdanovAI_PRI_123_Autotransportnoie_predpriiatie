using System;

namespace VehicleCompany.Models
{
    public class Route
    {
        public required long Id { get; set; }
        public required System.TimeSpan Travel_time { get; set; }
        public required long Price { get; set; }
    }
}
