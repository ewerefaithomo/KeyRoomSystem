using System.ComponentModel.DataAnnotations;

namespace KeyRoomSystem.Models
{
    public class KeyTransaction
    {
        public int Id { get; set; }

        [Required]
        public int KeyId { get; set; }

        [Required]
        [StringLength(50)]
        public string StaffNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateCollected { get; set; }

        public DateTime? DateReturned { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Collected";

        // Navigation properties
        public KeyItem? KeyItem { get; set; }

        public Staff? Staff { get; set; }
    }
}