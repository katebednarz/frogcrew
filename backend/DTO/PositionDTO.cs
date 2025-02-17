namespace backend.DTO;

public record PositionDTO()
{
    public int Id { get; init; }
    public string? Name { get; init; }
}