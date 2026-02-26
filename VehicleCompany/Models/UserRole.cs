using System.ComponentModel.DataAnnotations.Schema;
using VehicleCompany.Models;

namespace VehicleCompany.Models
{
    [Table("User_role")]
    public class UserRole
    {
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
}