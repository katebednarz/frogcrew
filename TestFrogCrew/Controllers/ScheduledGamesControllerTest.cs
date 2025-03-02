using Moq;
using Moq.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Controllers;
using backend.DTO;
using backend.Models;
using backend.Utils;
using misc;

namespace TestFrogCrew.Controllers;

[TestFixture]
public class ScheduledGamesControllerTest
{
    private FrogcrewContext _context;
    private ScheduledGamesController _controller;
    private DatabaseHelper? _dbHelper;
    private NotificationsHelper? _notificationsHelper;

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
        _context.Games.AddRange(
            new Game
            {
                Id = 1,
                GameStart = TimeOnly.Parse("6:30"),
                GameDate = DateOnly.Parse("2024-11-09"),
                Venue = "Amon G Carter",
                Opponent = "OSU",
                ScheduleId = 1
            },
            new Game
            {
                Id = 2,
                GameStart = TimeOnly.Parse("6:30"),
                GameDate = DateOnly.Parse("2024-11-10"),
                Venue = "Amon G Carter",
                Opponent = "OSU",
                ScheduleId = 1
            }
        );
        
        _context.Schedules.AddRange(
            new Schedule {Id = 1, Sport = "Basketball"}
        );
        
        _context.Users.AddRange(
            new ApplicationUser {Id = 1, FirstName = "Billy", LastName = "Bob"},
            new ApplicationUser {Id = 2, FirstName = "John", LastName = "Smith"},
            new ApplicationUser {Id = 3, FirstName = "Kate", LastName = "Bednarz"}
        );
        
        _context.CrewedUsers.AddRange(
            new CrewedUser {
                UserId = 1,
                GameId = 1,
                PositionId = 1,
                ArrivalTime = TimeOnly.Parse("6:30"),
            },
            new CrewedUser {
                UserId = 1,
                GameId = 2,
                PositionId = 1,
                ArrivalTime = TimeOnly.Parse("6:30"),
            }
        );
        
        _context.Positions.AddRange(
            new Position {PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM"},
            new Position {PositionId = 2, PositionName = "PRODUCER", PositionLocation = "CONTROL ROOM"}
        );
        
        _context.TradeBoards.AddRange(
            new TradeBoard {TradeId = 1, GameId = 1, DropperId = 1, Position = 1, Status = "AVAILABLE"},
            new TradeBoard {TradeId = 3, GameId = 1, DropperId = 1, ReceiverId = 2, Position = 1, Status = "AWAITING APPROVAL"}
        );
        
        _context.UserQualifiedPositions.AddRange(
            new UserQualifiedPosition {UserId = 1, PositionId = 1},
            new UserQualifiedPosition {UserId = 2, PositionId = 1},
            new UserQualifiedPosition {UserId = 3, PositionId = 2}
        );
        
        
        _context.SaveChanges();
        
        _notificationsHelper = new NotificationsHelper(_context);
        _controller = new ScheduledGamesController(_context, _notificationsHelper);
        _dbHelper = new DatabaseHelper(_context);
    }
    
    [TearDown]
    public void Teardown()
    {
        _context?.Dispose();
        _controller?.Dispose();
    }
    
    [Test]
    public async Task DropShiftSuccess()
    {
        // Arrange
        var gameId = 2;
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = 1,
            GameId = gameId,
            PositionId = 1,
        };
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Drop Success")); //Verify Message
            
        });
        
        // Verify data matches expected TradeBoardDTO structure
        var data = response?.Data as TradeBoardDTO;
        Assert.That(data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(data?.TradeId, Is.EqualTo(4));
            Assert.That(data?.DropperId, Is.EqualTo(1));
            Assert.That(data?.GameId, Is.EqualTo(gameId));
            Assert.That(data?.Position, Is.EqualTo("DIRECTOR"));
            Assert.That(data?.Status, Is.EqualTo("AVAILABLE"));
            Assert.That(data?.ReceiverId, Is.Null);
        });
    }
    
    [Test]
    public async Task DropShiftGameNotFound()
    {
        // Arrange
        var gameId = 3;
        
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = 1,
            GameId = gameId,
            PositionId = 1,
        };
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find game with id 3")); //Verify Message
            
        });
    }
    
    [Test]
    public async Task DropShiftUserNotFound()
    {
        // Arrange
        var gameId = 1;
        var userId = 4;
        
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = userId,
            GameId = gameId,
            PositionId = 1,
        };
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
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
    public async Task DropShiftShiftNotFound()
    {
        // Arrange
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = 1,
            GameId = 1,
            PositionId = 2,
        };
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find shift")); //Verify Message
            
        });
    }
    
    [Test]
    public async Task PickupShiftSuccess()
    {
        // Arrange
        var tradeId = 1;
        var userId = 2; 
        
        // Act
        var result = await _controller.PickupShift(tradeId,userId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Request Success")); //Verify Message
        });
        
        // Verify data matches expected TradeBoardDTO structure
        var data = response?.Data as TradeBoardDTO;
        Assert.That(data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(data?.TradeId, Is.EqualTo(1));
            Assert.That(data?.DropperId, Is.EqualTo(1));
            Assert.That(data?.GameId, Is.EqualTo(1));
            Assert.That(data?.Position, Is.EqualTo("DIRECTOR"));
            Assert.That(data?.Status, Is.EqualTo("AWAITING APPROVAL"));
            Assert.That(data?.ReceiverId, Is.EqualTo(2));
        });
    }
    
    [Test]
    public async Task PickupShiftTradeNotFound()
    {
        // Arrange 
        var tradeId = 2;
        var userId = 2; 
        
        // Act
        var result = await _controller!.PickupShift(tradeId,userId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find trade with id 2")); //Verify Message
            
        });
    }
    
    [Test]
    public async Task PickupShiftUserNotFound()
    {
        // Arrange 
        var tradeId = 1;
        var userId = 4; 
         
        // Act
        var result = await _controller.PickupShift(tradeId,userId) as ObjectResult;
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
    public async Task PickupShiftUserNotQualified()
    {
        // Arrange 
        var tradeId = 1;
        var userId = 3; 
        
        // Act
        var result = await _controller.PickupShift(tradeId,userId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("User is not qualified for this position")); //Verify Message
        });
    }
    
    [Test]
    public async Task ApproveShiftSuccess()
    {
        // Arrange
        var tradeId = 3;
        
        // Act
        var result = await _controller.ApproveShift(tradeId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Approval Success")); //Verify Message
        });
    }
    
    [Test]
    public async Task ApproveShiftTradeNotFound()
    {
        // Arrange 
        var tradeId = 2;
        
        // Act
        var result = await _controller!.ApproveShift(tradeId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        });
    }
    
    [Test]
    public async Task FindTradeBoards()
    {
        // Act
        var result = await _controller.GetTradeBoard() as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
        });
        
        // Verify data matches expected TradeBoardDTO structure
        var data = response?.Data as List<TradeBoardDTO>;
        Assert.That(data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(data[0]?.TradeId, Is.EqualTo(1));
            Assert.That(data[0]?.DropperId, Is.EqualTo(1));
            Assert.That(data[0]?.GameId, Is.EqualTo(1));
            Assert.That(data[0]?.Position, Is.EqualTo("DIRECTOR"));
            Assert.That(data[0]?.Status, Is.EqualTo("AVAILABLE"));
            Assert.That(data[0]?.ReceiverId, Is.Null);
        });
    }
}