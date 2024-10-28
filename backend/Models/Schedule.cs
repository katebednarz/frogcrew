using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace backend.Models;

public class Schedule
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int Id { get; set; }

    private string Sport { get; set; }
    private string Season { get; set; }
    private List<Game> Games { get; set; }

    public void CreateSchedule() { /* Implementation */ }
    public void ViewSchedule() { /* Implementation */ }
    public void PublishSchedule() { /* Implementation */ }

}
