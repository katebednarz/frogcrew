using System.Net;
using System.Net.Mail;
using backend.Controllers;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;

namespace backend.Utils;

public class NotificationsHelper
{
    private readonly FrogcrewContext _context;

    public NotificationsHelper(FrogcrewContext context)
    {
        _context = context;
    }
    
    public  void SendNotificationToAdmin(string notificationType)
    {
        SendNotificationToUser(1, notificationType, false);
    }

    public void SendNotificationToAllUsers(string notificationType, bool sendEmail = false)
    {
        var users = await _context.Users
            .Where(u => u.Availabilities.Any(a => a.GameId == gameId && a.Available == 1)
                        && u.UserQualifiedPositions.Any(qp => qp.PositionId == positionId))
            .Select(u => new UserSimpleDTO
            {
                UserId = u.Id,
                FullName = $"{u.FirstName} {u.LastName}"
            })
            .ToListAsync();
        
        
    }

    public  void SendNotificationToUser(int userId, string message, bool sendEmail)
    {
        var user = _context.Users.Find(userId);
        // create a new notification and add it to that userId's notifications
        Notification newNotif = new Notification()
        {
            UserId = userId,
            Message = message,
            Date = DateTime.Now,
            IsRead = false,
        };
        
        _context.Notifications.Add(newNotif);
        _context.SaveChanges();
        
        if (sendEmail)
        {
            SendEmailNotification(user.Email, message);
        }
    }

    public void SendEmailNotification(string email, string msg)
    {
        // email setup
        var fromAddress = new MailAddress("frog.crew.invitation@gmail.com", "FrogCrew");
        var toAddress = new MailAddress(email);
        const string fromPassword = "icbu ddnf yuhi lssz"; // gmail app key
        const string subject = "FrogCrew Notification";
        string body = msg;

        // configure SMTP client
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587, // SMTP port
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using (var message = new MailMessage(fromAddress, toAddress)
               {
                   Subject = subject,
                   Body = body
               })
        {
            smtp.Send(message);
        }
    }
}