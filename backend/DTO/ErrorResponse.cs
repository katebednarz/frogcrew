using System;

namespace backend.DTO;

public class ErrorResponse
{
    public bool Flag { get; set; }
    public int Code { get; set; }
    public string? Message { get; set; }
    public required Dictionary<string, string> Data { get; set; }
}
