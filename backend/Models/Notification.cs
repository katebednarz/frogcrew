using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Notification
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int Id { get; set; }

    private User User { get; set; }
    private string Title { get; set; }
    private string Content { get; set; }
    private DateTime Date { get; set; }

    public void ViewNotifications() { /* Implementation */ }

}
