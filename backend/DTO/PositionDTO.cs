using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public record PositionDTO()
{
    public int Id { get; init; }
    
    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; init; }
    
    //[Required(ErrorMessage = "Location is required.")]
    //public required string Location { get; set; }
}