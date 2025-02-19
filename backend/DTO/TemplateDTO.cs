using backend.Models;

namespace backend.DTO;

public record TemplateDTO()
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public List<Position> Positions { get; init; }
}