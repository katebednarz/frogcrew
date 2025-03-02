using Moq;
using Moq.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Controllers;
using backend.DTO;
using backend.Models;
using backend.Utils;
using misc;
using backend.Controllers;
using backend.Models;

namespace TestFrogCrew.Controllers;

[TestFixture]
public class NotificationControllerTest
{
    private FrogcrewContext _context;
    private NotificationController _controller;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<FrogcrewContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
      
        _context = new FrogcrewContext(options);
      
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
      
        // Adding Test Data
        _context.Notifications.AddRange(
            new Notification { Id = 1, UserId = 1, Message = "Test 1" },
            new Notification { Id = 2, UserId = 1, Message = "Test 2"},
            new Notification { Id = 3, UserId = 1, Message = "Test 3", IsRead = true}
        );
        
        _context.Users.AddRange(
            new ApplicationUser { Id = 1, FirstName = "Kate", LastName = "Bednarz"}
        );
        
        _context.SaveChanges();
      
        _controller = new NotificationController(_context);
    }
    
    [TearDown]
    public void Teardown()
    {
        _context?.Dispose();
        _controller?.Dispose();
    }

    [Test]
    public async Task GetNotificationsUserNotFound()
    {
        // Ararange
        var userId = 4;
        
        // Act
        var result = await _controller.GetNotifications(userId) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find user with id 4")); //Verify Message
        });
    }
    
    [Test]
    public async Task GetNotificationsSuccess()
    {
        // Arrange
        var userId = 1;
        
        // Act
        var result = await _controller.GetNotifications(userId) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
        });
        
        var data = response?.Data as List<NotificationDTO>;
        Assert.Multiple(() =>
        {
            Assert.That(data?[0].Id, Is.EqualTo(1));
            Assert.That(data?[0].Message, Is.EqualTo("Test 1"));
            Assert.That(data?[0].IsRead, Is.False);
            Assert.That(data?[1].Id, Is.EqualTo(2));
            Assert.That(data?[1].Message, Is.EqualTo("Test 2"));
            Assert.That(data?[1].IsRead, Is.False);
            Assert.That(data?[2].Id, Is.EqualTo(3));
            Assert.That(data?[2].Message, Is.EqualTo("Test 3"));
            Assert.That(data?[2].IsRead, Is.True);
        });
    }
    
    [Test]
    public async Task MarkNotificationAsReadNotificationNotFound()
    {
        // Arrange
        var notificationId = 4;
        
        
        // Act
        var result = await _controller.MarkNotificationAsRead(notificationId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find notification with id 4")); //Verify Message
        });
    }
    
    [Test]
    public async Task MarkNotificationAsReadSuccess()
    {
        // Arrange
        var notificationId = 1;
        
        // Act
        var result = await _controller.MarkNotificationAsRead(notificationId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Mark Success")); //Verify Message
        });
        
        var data = response?.Data as NotificationDTO;
        Assert.That(data?.IsRead, Is.True );
    }
    
    [Test]
    public async Task DeleteNotificationNotificationNotFound()
    {
        // Arrange
        var notificationId = 4;
        
        // Act
        var result = await _controller.DeleteNotification(notificationId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find notification with id 4")); //Verify Message
        });
    }
    
    [Test]
    public async Task DeleteNotificationSuccess()
    {
        // Arrange
        var notificationId = 1;
        
        // Act
        var result = await _controller.DeleteNotification(notificationId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Delete Success")); //Verify Message
        });
    }
}