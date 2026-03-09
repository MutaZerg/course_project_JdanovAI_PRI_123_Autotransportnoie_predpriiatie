using Microsoft.EntityFrameworkCore;
using VehicleCompany.Models;
using Route = VehicleCompany.Models.Route;
using User = VehicleCompany.Models.User;

namespace VehicleCompany.Contexts
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public DbSet<Route> Route { get; set; }
        public DbSet<Trip> Trip { get; set; }
        public DbSet<Stop> Stop { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Seat> Seat { get; set; }
        public DbSet<RouteStop> RouteStop { get; set; }
        public DbSet<Booking> Booking { get; set; } = default!;
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<RouteStop>().HasKey(rs => new { rs.RouteId, rs.StopId });
        }
    }
}
