using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("role_name")]
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}