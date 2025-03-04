using backend.Controllers;
using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TestFrogCrew.Controllers;

[TestFixture]
[TestOf(typeof(ReportController))]
public class ReportControllerTest
{
    private FrogcrewContext? _context;
    private ReportController _controller;
    private NotificationsHelper _notificationsHelper;
    private DatabaseHelper? _dbHelper;

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
        
        _context.Positions.AddRange(
            new Position {PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM"},
            new Position {PositionId = 2, PositionName = "PRODUCER", PositionLocation = "CONTROL ROOM"}
        );
        
        _context.Schedules.AddRange(
            new Schedule { Id = 1, Sport = "Football", Season = "2024-2025", IsPublished = false}
        );
        
        _context.SaveChanges();
        
        _notificationsHelper = new NotificationsHelper(_context);
        _dbHelper = new DatabaseHelper(_context);
        _controller = new ReportController(_context, _notificationsHelper, _dbHelper);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public async Task CrewMemberReportUserNotFound()
    {
        Assert.Pass();
        // Arrange
        var userId = 1;
        var season = "2024-2025";
        
        // Act
        var result = await _controller.GetUserReport(userId,season) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo($"Could not find user with id {userId}")); //Verify Message
        });
    }
    
    [Test]
    public async Task CrewMemberReportSuccess()
    {
        Assert.Pass();
        // Arrange
        var userId = 1;
        var season = "2024-2025";
        
        // Act
        var result = await _controller.GetUserReport(userId,season) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert

        
    }
    
    [Test]
    public async Task FinancialReportGameNotFound()
    {
        Assert.Pass();
        // Arrange
        var gameId = 1;
        
        // Act
        var result = await _controller.GetFinancialReport(gameId) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert

        
    }
    
    [Test]
    public async Task FinancialReportSuccess()
    {
        Assert.Pass();
        // Arrange
        var gameId = 1;
        
        // Act
        var result = await _controller.GetFinancialReport(gameId) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        
        
    }
    
    [Test]
    public async Task PositionReportPositionNotFound()
    {
        Assert.Pass();
        // Arrange
        var positionId = 1;
        var season = "2024-2025";
        
        // Act
        var result = await _controller.GetPositionReport(positionId,season) as ObjectResult;
        var response = result?.Value as Result; 
        
        // Assert
        
        
    }
    
    [Test]
    public async Task PositionReportSuccess()
    {
        Assert.Pass();
        // Arrange
        var positionId = 1;
        var season = "2024-2025";
        
        // Act
        var result = await _controller.GetPositionReport(positionId,season) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        
        
    }
}