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
    private Mock<FrogcrewContext>? _mockContext;
    private NotificationController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<FrogcrewContext>();
        _controller = new NotificationController(_mockContext.Object);
    }
    
    [TearDown]
    public void Teardown()
    {
        _controller?.Dispose();
    }
    
    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        // Setup IQueryable methods
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        // Setup IAsyncEnumerable method
        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return mockDbSet;
    }

    [Test]
    public async Task GetNotificationsUserNotFound()
    {
        // Arrange
        var notifications = new List<Notification>()
        {
            new Notification()
            {
                Id = 1,
                UserId = 1,
            },
            new Notification()
            {
                Id = 2,
                UserId = 1,
            },
            new Notification()
            {
                Id = 2,
                UserId = 1,
            },
        };
        var users = new List<ApplicationUser>();
        
        var mockUserSet = CreateMockDbSet(users);
        _mockContext?.Setup(m => m.Users).Returns(mockUserSet.Object);
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        //_mockContext?.Setup(m => m.Users.FindAsync(1)).;

        // Act
        var result = await _controller.GetNotifications(1) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find user with id 1")); //Verify Message
        });
    }
    
    [Test]
    public async Task GetNotificationsSuccess()
    {
        // Arrange
        var user = new ApplicationUser()
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
        };
        
        var notifications = new List<Notification>()
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Message = "Test 1",
                IsRead = false,
            },
            new()
            {
                Id = 2,
                UserId = 1,
                Message = "Test 2",
                IsRead = false,
            },
            new()
            {
                Id = 3,
                UserId = 1,
                Message = "Test 3",
                IsRead = true,
            },
        };
        
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        _mockContext?.Setup(m => m.Users.FindAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetNotifications(1) as ObjectResult;
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
        var notifications = new List<Notification>()
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Message = "Test 1",
                IsRead = false,
            }
        };
        
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        _mockContext?.Setup(m => m.Notifications.FindAsync(1)).ReturnsAsync(notifications[0]);
        
        // Act
        var result = await _controller.MarkNotificationAsRead(2) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find notification with id 2")); //Verify Message
        });
    }
    
    [Test]
    public async Task MarkNotificationAsReadSuccess()
    {
        // Arrange
        var notifications = new List<Notification>()
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Message = "Test 1",
                IsRead = false,
            }
        };
        
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        _mockContext?.Setup(m => m.Notifications.FindAsync(1)).ReturnsAsync(notifications[0]);
        
        // Act
        var result = await _controller.MarkNotificationAsRead(1) as ObjectResult;
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
        var notifications = new List<Notification>()
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Message = "Test 1",
                IsRead = false,
            }
        };
        
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        _mockContext?.Setup(m => m.Notifications.FindAsync(1)).ReturnsAsync(notifications[0]);
        
        // Act
        var result = await _controller.DeleteNotification(2) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find notification with id 2")); //Verify Message
        });
    }
    
    [Test]
    public async Task DeleteNotificationSuccess()
    {
        // Arrange
        var notifications = new List<Notification>()
        {
            new()
            {
                Id = 1,
                UserId = 1,
                Message = "Test 1",
                IsRead = false,
            }
        };
        
        var mockNotificationsSet = CreateMockDbSet(notifications);
        _mockContext?.Setup(m => m.Notifications).Returns(mockNotificationsSet.Object);
        _mockContext?.Setup(m => m.Notifications.FindAsync(1)).ReturnsAsync(notifications[0]);
        
        // Act
        var result = await _controller.DeleteNotification(1) as ObjectResult;
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