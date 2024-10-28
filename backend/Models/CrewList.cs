using System;

namespace backend.Models;

public class CrewList
{

    private int GameId { get; set; }
    private List<User> AssignedCrew { get; set; }
    private string Location { get; set; }
    private DateTime ReportTime { get; set; }

    public void GenerateCrewList() { /* Implementation */ }

}
