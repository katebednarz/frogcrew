using System;
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTO;

public class CrewedUserDTO
{
  public int UserId { get; set; }
  public int GameId { get; set; }
  public required string FullName { get; set; }
  public required string Position { get; set; }
  public required string ReportTime { get; set; }
}
