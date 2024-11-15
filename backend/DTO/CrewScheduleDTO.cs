using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class CrewScheduleDTO
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Changes to the crew schedule is required.")]
        public List<ChangesDTO>? Changes { get; set; }
    }
}