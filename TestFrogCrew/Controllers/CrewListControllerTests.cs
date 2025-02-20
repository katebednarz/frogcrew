using NUnit.Framework;
using backend.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using backend.DTO;
using backend.Utils;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using misc;
using Moq.EntityFrameworkCore;

namespace TestFrogCrew.Controllers;

  [TestFixture()]
  public class CrewListControllerTests
  {
    private Mock<FrogcrewContext>? _mockContext;
    private CrewListController? _controller;
    private DatabaseHelper? _dbHelper;


    [SetUp]
    public void Setup()
    {
      _mockContext = new Mock<FrogcrewContext>();
      _controller = new CrewListController(_mockContext.Object);
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

    [Test()]
    public async Task FindCrewListByIdTestSuccess()
    {
      // Arrange
      var gameId = 1;
      int positionId = 1;
      string positionName = "DIRECTOR";
      var game = new Game
      {
        Id = gameId,
        GameStart = TimeOnly.Parse("6:30"),
        GameDate = DateOnly.Parse("2024-11-09"),
        Venue = "Amon G Carter",
        Opponent = "OSU"
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

      var users = new List<ApplicationUser>
        {
            new() {
                Id = 1,
                FirstName = "John",
                LastName = "Smith"
            },
            new() {
                Id = 2,
                FirstName = "Jane",
                LastName = "Doe"
            }
        };

      // Mock DbSet for Games, CrewedUsers, and Users
      var mockCrewedUserSet = CreateMockDbSet(crewedUsers);
      var mockUserSet = CreateMockDbSet(users);

      _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(game);
      _mockContext?.Setup(c => c.CrewedUsers).Returns(mockCrewedUserSet.Object);
      _mockContext?.Setup(c => c.Users).Returns(mockUserSet.Object);

      _mockContext?.Setup(c => c.Positions)
        .ReturnsDbSet(new List<Position> { new() { PositionId = positionId, PositionName = positionName, PositionLocation = "CONTROL ROOM" } });
      
      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
      });

      // Verify data matches expected CrewListDTO structure
      var data = response?.Data as CrewListDTO;
      Assert.That(data, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(data?.GameId, Is.EqualTo(gameId));
        Assert.That(data?.GameStart.ToString(), Is.EqualTo(TimeOnly.Parse("6:30").ToString()));
        Assert.That(data?.GameDate.ToString(), Is.EqualTo(DateOnly.Parse("2024-11-09").ToString()));
        Assert.That(data?.Venue, Is.EqualTo("Amon G Carter"));
        Assert.That(data?.Opponent, Is.EqualTo("OSU"));
      });

      // Verify crew members in CrewListDTO
      Assert.That(data?.CrewedMembers, Has.Count.EqualTo(2));

      Assert.Multiple(() =>
      {
        // Check the first crew member
        Assert.That(data?.CrewedMembers?[0].UserId, Is.EqualTo(1));
        Assert.That(data?.CrewedMembers?[0].Position, Is.EqualTo("DIRECTOR"));
      });

      Assert.Multiple(() =>
      {
        // Check the second crew member
        Assert.That(data?.CrewedMembers?[1].UserId, Is.EqualTo(2));
        Assert.That(data?.CrewedMembers?[1].Position, Is.EqualTo("DIRECTOR"));
      });
    }

    [Test()]
    public async Task FindCrewListByIdTestNotFound()
    {
      var gameId = 1;
      #pragma warning disable CS8600
      _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync((Game)null);
      #pragma warning restore CS8602

      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo($"Game with ID {gameId} not found.")); //Verify Message
      });

      // Verify FindAsync was called once
      _mockContext?.Verify(c => c.Games.FindAsync(gameId), Times.Once);
    }

    [Test()]
    public async Task ExportCrewListGameNotFound()
    {
      // Arrange
      var gameId = 1;
      _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync((Game)null);
      
      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;
      
      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo($"Game with ID {gameId} not found.")); //Verify Message
      });
    }

    [Test()]
    public async Task ExportCrewListSuccess()
    {
      // Arrange
      var gameId = 1;
      
      var game = new Game
      {
        Id = gameId,
        Opponent = "Texas",
        GameDate = new DateOnly(2024, 11, 06),
        GameStart = new TimeOnly(18,0),
        Schedule = new Schedule { Sport = "Basketball" }
      };

      var user = new ApplicationUser
      {
        Id = 1,
        FirstName = "John",
        LastName = "Smith",
      };

      var crewedUsers = new List<CrewedUser>
      {
        new()
        {
          UserId = 1,
          GameId = gameId,
          Game = game,
          CrewedPositionNavigation = new Position
          {
            PositionId = 1,
            PositionName = "DIRECTOR",
            PositionLocation = "CONTROL ROOM"
          },
          ArrivalTime = new TimeOnly(17, 0),
        }
      };
      
      var mockCrewedUserSet = CreateMockDbSet(crewedUsers);
      _mockContext?.Setup(c => c.CrewedUsers).Returns(mockCrewedUserSet.Object);
      
      _mockContext?.Setup(c => c.Games).ReturnsDbSet(new List<Game> { game });
      _mockContext?.Setup(c => c.Users).ReturnsDbSet(new List<ApplicationUser> { user });
      
      // Act
      var result = await _controller!.ExportCrewList(gameId) as FileContentResult;
      
      //Assert
      Assert.IsNotNull(result);
      Assert.That(result.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
      Assert.That(result.FileDownloadName, Is.EqualTo("CrewList.xlsx"));

      // Verify the Excel file structure
      using (var stream = new MemoryStream(result.FileContents))
      using (var workbook = new XLWorkbook(stream))
      {
        var worksheet = workbook.Worksheet(1);
        Assert.That(worksheet.Cell("A1").Value, Is.EqualTo("TCU SPORTS BROADCASTING CREW LIST"));
        Assert.That(worksheet.Cell("A2").Value, Is.EqualTo("TCU BASKETBALL vs Texas"));
        Assert.That(worksheet.Cell("A3").Value, Is.EqualTo("Wednesday 11-06-24"));
        Assert.That(worksheet.Cell("A4").Value, Is.EqualTo("6:00 PM"));
        Assert.That(worksheet.Cell("A8").Value, Is.EqualTo("DIRECTOR"));
        Assert.That(worksheet.Cell("B8").Value, Is.EqualTo("John Smith"));
        Assert.That(worksheet.Cell("C8").Value, Is.EqualTo("17:00"));
        Assert.That(worksheet.Cell("D8").Value, Is.EqualTo("CONTROL ROOM"));

      }
      


    }
  }
