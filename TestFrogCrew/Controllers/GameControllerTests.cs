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
using Microsoft.EntityFrameworkCore;
using misc;

namespace backend.Controllers.Tests
{
  [TestFixture()]
  public class GameControllerTests
  {
    private Mock<FrogcrewContext>? _mockContext;
    private GameController? _controller;

    [SetUp]
    public void Setup()
    {
      _mockContext = new Mock<FrogcrewContext>();
      _controller = new GameController(_mockContext.Object);
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

      // Setup for synchronous queryable
      mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
      mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
      mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
      mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

      // Setup for asynchronous queryable
      mockDbSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
          .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

      mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

      return mockDbSet;
    }

    [Test()]
    public async Task FindGameByIdTestSuccess()
    {
      // Arrange
      var gameId = 1;
      var game = new Game
      {
        Id = gameId,
        ScheduleId = 1,
        GameDate = new DateOnly(2024, 11, 06),
        Venue = "Amon G Carter",
        Opponent = "Texas Tech",
        IsFinalized = false
      };

      _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(game);

      // Act
      var result = await _controller!.FindGameById(gameId) as ObjectResult;
      var response = result?.Value as Result;
      // Assert
      Assert.Multiple(() =>
      {

        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
      });

      // Check that data is correctly returned as a GameDTO
      var gameDTO = response?.Data as GameDTO;
      Assert.That(gameDTO, Is.Not.Null);
      Assert.That(gameDTO?.GameId, Is.EqualTo(game.Id));

      // Verify FindAsync was called once
      _mockContext?.Verify(c => c.Games.FindAsync(gameId), Times.Once);
    }

    [Test()]
    public async Task FindGameByIdTestNotFound()
    {
      // Arrange
      var gameId = 1;
      #pragma warning disable CS8600
      _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync((Game)null);
      #pragma warning restore CS8602

      // Act
      var result = await _controller!.FindGameById(gameId) as ObjectResult;
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
    public async Task FindGamesByScheduleIdTestSuccess()
    {
      // Arrange
      int scheduleId = 1;
      var games = new List<Game>
        {
            new() {
              Id = 1,
              ScheduleId = scheduleId,
              GameDate = DateOnly.Parse("2024-11-09"),
              Venue = "Amon G Carter",
              Opponent = "OSU"
            },
            new() {
              Id = 2,
              ScheduleId = scheduleId,
              GameDate = DateOnly.Parse("2024-11-10"),
              Venue = "Amon G Carter",
              Opponent = "SMU"
            }
        };

      var mockGameSet = CreateMockDbSet(games);
      _mockContext?.Setup(c => c.Games).Returns(mockGameSet.Object);


      // Act
      var result = await _controller!.FindGamesByScheduleId(scheduleId) as ObjectResult;
      var response = result?.Value as Result;
      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Found Games")); //Verify Message
      });

      // Verify data returned as a list of GameDTOs
      var gameDTOs = response?.Data as List<GameDTO>;
      Assert.That(gameDTOs, Is.Not.Null);
      Assert.That(gameDTOs, Has.Count.EqualTo(2));

      // Verify properties of the first game
      Assert.Multiple(() =>
      {
        Assert.That(gameDTOs?[0].GameId, Is.EqualTo(games[0].Id));
        Assert.That(gameDTOs?[0].ScheduleId, Is.EqualTo(games[0].ScheduleId));
        Assert.That(gameDTOs?[0].GameDate, Is.EqualTo(games[0].GameDate));
        Assert.That(gameDTOs?[0].Venue, Is.EqualTo(games[0].Venue));
        Assert.That(gameDTOs?[0].Opponent, Is.EqualTo(games[0].Opponent));
      });

      // Verify properties of the second game
      Assert.Multiple(() =>
      {
        Assert.That(gameDTOs?[1].GameId, Is.EqualTo(games[1].Id));
        Assert.That(gameDTOs?[1].ScheduleId, Is.EqualTo(games[1].ScheduleId));
        Assert.That(gameDTOs?[1].GameDate, Is.EqualTo(games[1].GameDate));
        Assert.That(gameDTOs?[1].Venue, Is.EqualTo(games[1].Venue));
        Assert.That(gameDTOs?[1].Opponent, Is.EqualTo(games[1].Opponent));
      });
    }

    [Test()]
    public async Task FindGamesByScheduleIdTestNotFound()
    {
      // Arrange
      var scheduleId = 1;
      #pragma warning disable CS8600
      _mockContext?.Setup(c => c.Schedules.FindAsync(scheduleId)).ReturnsAsync((Schedule)null);
      #pragma warning restore CS8602
      

      // Act
      var result = await _controller!.FindGamesByScheduleId(scheduleId) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo($"Could not find schedule with Id {scheduleId}.")); //Verify Message
      });

      // Verify data is null or empty
      Assert.That(response?.Data, Is.Null);
    }
  }
}