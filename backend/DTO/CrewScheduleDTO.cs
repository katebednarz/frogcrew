using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class CrewScheduleDTO
    {
        public int GameId { get; set; }
        public List<ChangesDTO>? Changes { get; set; }
    }
}