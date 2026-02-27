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
        public DbSet<VehicleCompany.Models.Booking> Booking { get; set; } = default!;
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys for join tables (no single Id in model)
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<RouteStop>().HasKey(rs => new { rs.RouteId, rs.StopId });
        }

        //private void SeedData(ModelBuilder modelBuilder)
        //{
        //    // Seed default permissions
        //    modelBuilder.Entity<Permission>().HasData(
        //        new Permission { Id = 1, Action = "dashboard.view" },
        //        new Permission { Id = 2, Action = "users.view" },
        //        new Permission { Id = 3, Action = "users.edit" },
        //        new Permission { Id = 4, Action = "users.delete" },
        //        new Permission { Id = 5, Action = "roles.view" },
        //        new Permission { Id = 6, Action = "roles.edit" },
        //        new Permission { Id = 7, Action = "settings.view" }
        //    );

        //    // Seed default roles
        //    modelBuilder.Entity<Role>().HasData(
        //        new Role { Id = 1, RoleName = "Admin" },
        //        new Role { Id = 2, RoleName = "User" },
        //        new Role { Id = 3, RoleName = "Manager" }
        //    );

        //    // Seed admin user (password: Admin@123)
        //    modelBuilder.Entity<User>().HasData(
        //        new User
        //        {
        //            Id = 1,
        //            UserName = "admin",
        //            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        //            Email = "admin@example.com" // Заглушка
        //        }
        //    );

        //    // Assign admin role to admin user
        //    modelBuilder.Entity<UserRole>().HasData(
        //        new UserRole { UserId = 1, RoleId = 1 }
        //    );

        //    // Assign all permissions to admin role
        //    for (int i = 1; i <= 7; i++)
        //    {
        //        modelBuilder.Entity<RolePermission>().HasData(
        //            new RolePermission { RoleId = 1, PermissionId = i }
        //        );
        //    }
        //}
    }
}
