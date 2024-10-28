using System;

namespace backend.Models;

public class Availability
{

    private User User { get; set; }
    private Game Game { get; set; }
    private bool Open { get; set; }
    private string Comment { get; set; }

    public void SubmitAvailability() { /* Implementation */ }
    public void EditAvailability() { /* Implementation */ }

}
