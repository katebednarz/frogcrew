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

namespace backend.Controllers.Tests
{
  [TestFixture()]
  public class CrewListControllerTests
  {
    private Mock<FrogcrewContext>? _mockContext;
    private CrewListController? _controller;


    [SetUp]
    public void Setup()
    {
      _mockContext = new Mock<FrogcrewContext>();
      _controller = new CrewListController(_mockContext.Object);
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

    [Test()]
    public async Task FindCrewListByIdTestSuccess()
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

      var crewedUsers = new List<CrewedUser>
        {
            new() {
                UserId = 1,
                GameId = gameId,
                CrewedPosition = "DIRECTOR",
                ArrivalTime = TimeOnly.Parse("6:30"),
            },
            new() {
                UserId = 2,
                GameId = gameId,
                CrewedPosition = "DIRECTOR",
                ArrivalTime = TimeOnly.Parse("6:30"),
            }
        };

      var users = new List<User>
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
        Assert.That(data?.CrewedMembers?[0].FullName, Is.EqualTo("John Smith"));
      });

      Assert.Multiple(() =>
      {
        // Check the second crew member
        Assert.That(data?.CrewedMembers?[1].UserId, Is.EqualTo(2));
        Assert.That(data?.CrewedMembers?[1].Position, Is.EqualTo("DIRECTOR"));
        Assert.That(data?.CrewedMembers?[1].FullName, Is.EqualTo("Jane Doe"));
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
  }
}