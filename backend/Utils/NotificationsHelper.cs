using System.Net;
using System.Net.Mail;
using backend.Controllers;
using backend.Models;
using Microsoft.AspNetCore.Identity;
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
    
    public void SendNotificationToAdmin(string message)
    {
        var user = _context.Users.Find(1);
        // create a new notification and add it to that userId's notifications
        Notification newNotif = new Notification()
        {
            UserId = 1,
            Message = message,
            Date = DateTime.Now,
            IsRead = false,
        };
        
        _context.Notifications.Add(newNotif);
        _context.SaveChanges();
    }
    
    public  void SendNotificationToCrewMember(int userId, string message)
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
        
        // SendEmailNotification(user.Email, message);
    }

    public void SendNotificationToAllCrewMembers(string notificationType, bool sendEmail)
    {
        var users = from u in _context.Users
            join ur in _context.UserRoles on u.Id equals ur.UserId
            join r in _context.Roles on ur.RoleId equals r.Id
            where u.IsActive == true && r.Name != "ADMIN"
            select new
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                RoleId = r.Id,
                RoleName = r.Name
            };

        var results = users.ToList();
        foreach (var user in results)
        {
            SendNotificationToCrewMember(user.UserId, notificationType);
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