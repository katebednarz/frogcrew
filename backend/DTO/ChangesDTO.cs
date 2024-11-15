using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class ChangesDTO
    {
        [Required(ErrorMessage = "Action is required.")]
        public string Action { get; set; }
        [Required(ErrorMessage = "User ID is required.")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Position is required.")]
        public string Position { get; set; }
        public string? FullName { get; set; }
    }
}