using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class CrewScheduleDTO
    {

        public int id { get; set; }
        [Required(ErrorMessage = "Changes to the crew schedule is required.")]
        public List<ChangesDTO> changes { get; set; }
    }
}