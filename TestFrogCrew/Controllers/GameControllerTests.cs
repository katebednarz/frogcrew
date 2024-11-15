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

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return mockSet;
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
      Assert.Pass();
    }

    [Test()]
    public async Task FindGamesByScheduleIdTestNotFound()
    {
      Assert.Pass();
    }
  }
}