using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleCompany.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("User_name")]
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Column("Password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; //заглушка

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}