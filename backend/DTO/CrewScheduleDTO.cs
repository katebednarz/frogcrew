using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class CrewScheduleDTO
    {
        public int gameId { get; set; }
        public List<ChangesDTO>? changes { get; set; }
    }
}