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
    private Mock<FrogcrewContext>? _mockContext;
    private ScheduledGamesController _controller;
    private DatabaseHelper? _dbHelper;
    private NotificationsHelper? _notificationsHelper;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<FrogcrewContext>();
        _notificationsHelper = new NotificationsHelper(_mockContext.Object);
        _controller = new ScheduledGamesController(_mockContext.Object, _notificationsHelper);
        _dbHelper = new DatabaseHelper(_mockContext.Object);
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
            .ReturnsDbSet(new List<Position> { new() { PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM" } });
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
        // Arrange
        var gameId = 1;
        
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = 1,
            GameId = gameId,
            PositionId = 1,
        };
        
        var tradeBoards = new List<TradeBoard>();
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(null as Game);
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find game with id 1")); //Verify Message
            
        });
    }
    
    [Test]
    public async Task DropShiftUserNotFound()
    {
        // Arrange
        var gameId = 1;
        var userId = 1;
        var game = new Game
        {
            Id = gameId,
            GameStart = TimeOnly.Parse("6:30"),
            GameDate = DateOnly.Parse("2024-11-09"),
            Venue = "Amon G Carter",
            Opponent = "OSU"
        };
        
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = userId,
            GameId = gameId,
            PositionId = 1,
        };
        var tradeBoards = new List<TradeBoard>();
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(game);
        _mockContext?.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(null as ApplicationUser);
        
        // Act
        var result = await _controller!.DropShift(tradeInfo) as ObjectResult;
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
    public async Task DropShiftShiftNotFound()
    {
        // Arrange
        var gameId = 1;
        var userId = 1;
        var game = new Game
        {
            Id = gameId,
            GameStart = TimeOnly.Parse("6:30"),
            GameDate = DateOnly.Parse("2024-11-09"),
            Venue = "Amon G Carter",
            Opponent = "OSU"
        };
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "Billy",
            LastName = "Bob",
        };
        
        var tradeInfo = new TradeRequestInfo()
        {
            UserId = userId,
            GameId = gameId,
            PositionId = 1,
        };
        var tradeBoards = new List<TradeBoard>();
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(game);
        _mockContext?.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(user);
        _mockContext?.Setup(c => c.CrewedUsers.FindAsync()).ReturnsAsync(null as CrewedUser);
        
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
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = 1,
                FirstName = "Billy",
                LastName = "Bob"
            },
            new()
            {
                Id = 2,
                FirstName = "Joe",
                LastName = "Smith"
            }
        };
        
        var userQualifiedPositions = new List<UserQualifiedPosition>
        {
            new()
            {
                UserId = 1,
                PositionId = 1
            },
            new()
            {
                UserId = 2,
                PositionId = 1
            }
        };
        
        var tradeBoards = new List<TradeBoard>
        {
            new()
            {
                TradeId = 1,
                DropperId = 1,
                GameId = 1,
                Position = 1,
                Status = "AVAILABLE",
                ReceiverId = null,
            }
        };

        var positions = new List<Position>
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR",
                PositionLocation = "CONTROL ROOM"
            },
            new()
            {
                PositionId = 2,
                PositionName = "PRODUCER",
                PositionLocation = "CONTROL ROOM"
            }
        };
        
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        var mockUserSet = CreateMockDbSet(users);
        var mockPositionSet = CreateMockDbSet(positions);
        var mockUserQualifiedPositionSet = CreateMockDbSet(userQualifiedPositions);
        
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        _mockContext?.Setup(c => c.UserQualifiedPositions).Returns(mockUserQualifiedPositionSet.Object);
        _mockContext?.Setup(c => c.TradeBoards.FindAsync(1)).ReturnsAsync(tradeBoards.FirstOrDefault(u => u.TradeId == 1));
        _mockContext?.Setup(c => c.Users.FindAsync(2)).ReturnsAsync(users.FirstOrDefault(u => u.Id == 2));
        
        // Act
        var result = await _controller.PickupShift(1,2) as ObjectResult;
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
        var tradeBoards = new List<TradeBoard>();
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        
        // Act
        var result = await _controller!.PickupShift(1,2) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find trade with id 1")); //Verify Message
            
        });
    }
    
    [Test]
    public async Task PickupShiftUserNotFound()
    {
        // Arrange 
        var tradeBoards = new List<TradeBoard>
        {
            new()
            {
                TradeId = 1,
                DropperId = 1,
                GameId = 1,
                Position = 1,
                Status = "AVAILABLE",
                ReceiverId = null,
            }
        };
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Users.FindAsync(1)).ReturnsAsync(null as ApplicationUser);
        _mockContext?.Setup(c => c.TradeBoards.FindAsync(1)).ReturnsAsync(tradeBoards.FirstOrDefault(u => u.TradeId == 1));
         
        // Act
        var result = await _controller.PickupShift(1,1) as ObjectResult;
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
    public async Task PickupShiftUserNotQualified()
    {
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = 1,
                FirstName = "Billy",
                LastName = "Bob"
            },
            new()
            {
                Id = 2,
                FirstName = "Joe",
                LastName = "Smith"
            }
        };
        
        var userQualifiedPositions = new List<UserQualifiedPosition>
        {
            new()
            {
                UserId = 1,
                PositionId = 1
            },
            new()
            {
                UserId = 2,
                PositionId = 2
            }
        };
        
        var tradeBoards = new List<TradeBoard>
        {
            new()
            {
                TradeId = 1,
                DropperId = 1,
                GameId = 1,
                Position = 1,
                Status = "AVAILABLE",
                ReceiverId = null,
            }
        };

        var positions = new List<Position>
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR",
                PositionLocation = "CONTROL ROOM"
            },
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR",
                PositionLocation = "CONTROL ROOM"
            }
            
        };
        
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        var mockUserSet = CreateMockDbSet(users);
        var mockPositionSet = CreateMockDbSet(positions);
        var mockUserQualifiedPositionSet = CreateMockDbSet(userQualifiedPositions);
        
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        _mockContext?.Setup(c => c.UserQualifiedPositions).Returns(mockUserQualifiedPositionSet.Object);
        _mockContext?.Setup(c => c.TradeBoards.FindAsync(1)).ReturnsAsync(tradeBoards.FirstOrDefault(u => u.TradeId == 1));
        _mockContext?.Setup(c => c.Users.FindAsync(2)).ReturnsAsync(users.FirstOrDefault(u => u.Id == 2));
        
        // Act
        var result = await _controller.PickupShift(1,2) as ObjectResult;
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
        var tradeBoards = new List<TradeBoard>
        {
            new()
            {
                TradeId = 1,
                DropperId = 1,
                GameId = 1,
                Position = 1,
                Status = "AWAITING APPROVAL",
                ReceiverId = 2,
            }
        };
        
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = 1,
                FirstName = "Billy",
                LastName = "Bob"
            },
            new()
            {
                Id = 2,
                FirstName = "Joe",
                LastName = "Smith"
            }
        };

        var crewedUsers = new List<CrewedUser>
        {
            new()
            {
                UserId = 1,
                GameId = 1,
                PositionId = 1,
                ArrivalTime = TimeOnly.Parse("6:30"),
            }
        };
        
        var positions = new List<Position>
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR",
                PositionLocation = "CONTROL ROOM"
            },
            new()
            {
                PositionId = 2,
                PositionName = "PRODUCER",
                PositionLocation = "CONTROL ROOM"
            }
        };
        
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        var mockUserSet = CreateMockDbSet(users);
        var mockCrewedUserSet = CreateMockDbSet(crewedUsers);
        
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext?.Setup(c => c.CrewedUsers).Returns(mockCrewedUserSet.Object);
        _mockContext?.Setup(c => c.TradeBoards.FindAsync(1)).ReturnsAsync(tradeBoards.FirstOrDefault(u => u.TradeId == 1));
        _mockContext?.Setup(c => c.CrewedUsers.FindAsync(1, 1, 1))
            .ReturnsAsync(crewedUsers.FirstOrDefault(u => u.UserId == 1));
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        // Act
        var result = await _controller.ApproveShift(1) as ObjectResult;
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
        var tradeBoards = new List<TradeBoard>();
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        
        // Act
        var result = await _controller!.PickupShift(1,2) as ObjectResult;
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
        var tradeBoards = new List<TradeBoard>
        {
            new()
            {
                TradeId = 1,
                DropperId = 1,
                GameId = 1,
                Position = 1,
                Status = "AVAILABLE",
                ReceiverId = 2,
            },
            new()
            {
                TradeId = 2,
                DropperId = 1,
                GameId = 2,
                Position = 1,
                Status = "AVAILABLE",
                ReceiverId = 2,
            },
            new()
            {
                TradeId = 3,
                DropperId = 1,
                GameId = 3,
                Position = 1,
                Status = "AWAITING APPROVAL",
                ReceiverId = 2,
            }
        };
        
        var positions = new List<Position>
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR",
                PositionLocation = "CONTROL ROOM"
            },
            new()
            {
                PositionId = 2,
                PositionName = "PRODUCER",
                PositionLocation = "CONTROL ROOM"
            }
        };
        
        var mockTradeBoardSet = CreateMockDbSet(tradeBoards);
        var mockPositionSet = CreateMockDbSet(positions);
        
        _mockContext?.Setup(c => c.TradeBoards).Returns(mockTradeBoardSet.Object);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
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
    }
}