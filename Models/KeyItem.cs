using System.ComponentModel.DataAnnotations;

namespace KeyRoomSystem.Models
{
    public class KeyItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Key Number")]
        public string KeyNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Room Name")]
        public string RoomName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}