using System.ComponentModel.DataAnnotations;

namespace KeyRoomSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string Username { get; set; }
        [Required, DataType(DataType.Password)] public string Password { get; set; }
        [Display(Name = "Full Name")] public string FullName { get; set; }
    }
}