using System.ComponentModel.DataAnnotations.Schema;
using VehicleCompany.Models;

namespace VehicleCompany.Models
{
    [Table("Role_premission")]
    public class RolePermission
    {
        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("premission_id")]
        public long PermissionId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
    }
}