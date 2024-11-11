using System;
using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTO;

public class CrewedUserDTO
{
  public int UserId { get; set; }
  public string FullName { get; set; }
  public string Position { get; set; }
  public string ReportTime { get; set; }
}
