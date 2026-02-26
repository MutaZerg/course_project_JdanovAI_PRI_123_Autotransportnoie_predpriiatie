using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("Premission")]
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("action")]
        [Required]
        [StringLength(200)]
        public string Action { get; set; } = string.Empty; // Например: "table1", "users.view", "users.edit"

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}