using System;
using backend.Models;

namespace backend.DTO;

public class CrewMemberResponse
{
    public bool Flag { get; set; }
    public int Code { get; set; }
    public string Message { get; set; }
    public User Data { get; set; }
}
