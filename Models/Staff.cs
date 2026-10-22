using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeyRoomSystem.Models
{
    [Table("Staffs")]
    public class Staff
    {
        [Key]
        [Required(ErrorMessage = "Staff Number is required.")]
        [StringLength(50)]
        [Column("StaffNumber")]
        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; } = string.Empty;


        [StringLength(200)]
        [Column("FullName")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }


        [StringLength(150)]
        [Column("Department")]
        [Display(Name = "Department")]
        public string? Department { get; set; }


        [StringLength(30)]
        [Column("PhoneNumber")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}