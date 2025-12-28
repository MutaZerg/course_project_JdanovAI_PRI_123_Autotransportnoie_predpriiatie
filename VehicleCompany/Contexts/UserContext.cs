using VehicleCompany.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using Route = VehicleCompany.Models.Route;


namespace VehicleCompany.Contexts
{
    public class UserContext: DbContext
    {
        public UserContext(DbContextOptions<UserContext> options):base(options) 
        {

        }
        public DbSet<User> User { get; set; }
        public DbSet<Route> Route { get; set; }
        public DbSet<Trip> Trip { get; set; }
        public DbSet<Stop> Stop { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<VehicleCompany.Models.Booking> Booking { get; set; } = default!;
        //public DbSet<User> User { get; set; }
        //public DbSet<User> User { get; set; }


    }
}
