using Moq;
using Moq.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Controllers;
using backend.DTO;
using backend.Models;
using backend.Utils;

namespace TestFrogCrew.Controllers;

[TestFixture]
public class ScheduledGamesControllerTest
{
    private Mock<FrogcrewContext>? _mockContext;
    private ScheduledGamesController _controller;
    private DatabaseHelper? _dbHelper;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<FrogcrewContext>();
        _controller = new ScheduledGamesController(_mockContext.Object);
        _dbHelper = new DatabaseHelper(_mockContext.Object);
    }
    
    [TearDown]
    public void Teardown()
    {
        _controller?.Dispose();
    }
    
    private static Mock<DbSet<T>> CreateMockDbSet<T>(IList<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockDbSet;
    }

    [Test]
    public async Task DropShiftSuccess()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Id = gameId,
            GameStart = TimeOnly.Parse("6:30"),
            GameDate = DateOnly.Parse("2024-11-09"),
            Venue = "Amon G Carter",
            Opponent = "OSU"
        };
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = 1,
                FirstName = "Billy",
                LastName = "Bob",
                UserQualifiedPositions = new List<UserQualifiedPosition>
                {
                    new() { PositionId = 1 }
                }
            },
            new()
            {
                Id = 2,
                FirstName = "John",
                LastName = "Smith",
                UserQualifiedPositions = new List<UserQualifiedPosition>
                {
                    new() { PositionId = 1 }
                }
            }
        };
        var crewedUsers = new List<CrewedUser>
        {
            new() {
                UserId = 1,
                GameId = gameId,
                PositionId = 1,
                ArrivalTime = TimeOnly.Parse("6:30"),
            },
            new() {
                UserId = 2,
                GameId = gameId,
                PositionId = 1,
                ArrivalTime = TimeOnly.Parse("6:30"),
            }
        };
        var tradeBoards = new List<TradeBoard>();
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = 1,
            GameId = gameId,
            PositionId = 1,
        };
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        var mockCrewedUserSet = CreateMockDbSet(crewedUsers);
        var mockUserSet = CreateMockDbSet(users);
        
        _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(game);
        _mockContext?.Setup(c => c.CrewedUsers).Returns(mockCrewedUserSet.Object);
        _mockContext?.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Positions)
            .ReturnsDbSet(new List<Position> { new() { PositionId = 1, PositionName = "DIRECTOR" } });
        _mockContext?.Setup(c => c.Users.FindAsync(1))
            .ReturnsAsync(users.FirstOrDefault(u => u.Id == 1));
        _mockContext?.Setup(c => c.CrewedUsers.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] ids) =>
            {
                var userId = (int)ids[0];
                var gameId = (int)ids[1];
                var positionId = (int)ids[2];

                return crewedUsers.FirstOrDefault(cu =>
                    cu.UserId == userId &&
                    cu.GameId == gameId &&
                    cu.PositionId == positionId);
            });
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
            Assert.That(data?.TradeId, Is.EqualTo(0));
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
        Assert.Pass();
    }
    
    [Test]
    public async Task DropShiftUserNotFound()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task DropShiftShiftNotFound()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task PickupShiftSuccess()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task PickupShiftTradeNotFound()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task PickupShiftUserNotFound()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task PickupShiftUserNotQualified()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task ApproveShiftSuccess()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task ApproveShiftTradeNotFound()
    {
        Assert.Pass();
    }
}