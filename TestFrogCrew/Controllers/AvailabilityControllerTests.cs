using backend.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using Microsoft.EntityFrameworkCore;

namespace TestFrogCrew.Controllers;

  [TestFixture()]
  public class AvailabilityControllerTests
  {
    private Mock<FrogcrewContext>? _mockContext;
    private AvailabilityController? _controller;


    [SetUp]
    public void Setup()
    {
      _mockContext = new Mock<FrogcrewContext>();
      _controller = new AvailabilityController(_mockContext.Object);
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
    public async Task SubmitAvailabilityTestSuccess()
    {
      // Arrange
      var request = new List<AvailabilityDTO>
      {
        new() {
          UserId = 1,
          GameId = 1,
          Available = true,
          Comments = "Coming from another game, will be 30 mins late."
        },
        new() {
          UserId = 1,
          GameId = 2,
          Available = false,
          Comments = null
        }
      };
      
      var availabilityList = new List<Availability>
      {
        new() {
          UserId = 1,
          GameId = 1,
          Available = true,
          Comments = "Coming from another game, will be 30 mins late."
        },
        new() {
          UserId = 1,
          GameId = 2,
          Available = false,
          Comments = null
        }
      };

      var mockDbSet = CreateMockDbSet(availabilityList);
      
      _mockContext?.Setup(c => c.Availabilities).Returns(mockDbSet.Object);

      // Act
      var result = await _controller!.SubmitAvailability(request) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Add Success")); //Verify Message
      });

      // Check data returned as AvailabilityDTO
      var data = response?.Data as List<AvailabilityDTO>;
      Assert.That(data, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(data?.Count, Is.EqualTo(2));
        Assert.That(data?[0].UserId, Is.EqualTo(1));
        Assert.That(data?[0].GameId, Is.EqualTo(1));
        Assert.That(data?[0].Available, Is.EqualTo(true));
        Assert.That(data?[0].Comments, Is.EqualTo("Coming from another game, will be 30 mins late."));
        Assert.That(data?[1].UserId, Is.EqualTo(1));
        Assert.That(data?[1].GameId, Is.EqualTo(2));
        Assert.That(data?[1].Available, Is.EqualTo(false));
        Assert.That(data?[1].Comments, Is.EqualTo(null));
      });
    }

    [Test()]
    public async Task SubmitAvailabilityTestBadRequest()
    {
      Assert.Pass();
      // Arrange
      var request = new List<AvailabilityDTO>
      {
      };

      _controller?.ModelState.AddModelError("userId", "userId is required.");
      _controller?.ModelState.AddModelError("gameId", "gameId is required.");
      _controller?.ModelState.AddModelError("open", "open is required.");

      // Act
      var result = await _controller!.SubmitAvailability(request) as ObjectResult;
      var response = result?.Value as Result;

      // Expected data
      var expectedData = new Dictionary<string, string>
        {
            { "userId", "userId is required." },
            { "gameId", "gameId is required." },
            { "open", "open is required." }
        };

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); //Verify Message
      });

      // Check that the data contains the expected error messages
      Assert.That(response?.Data, Is.Not.Null);
      var errors = response?.Data as List<string>;
      foreach (var error in expectedData)
      {
        Assert.That(errors?.Any(e => e.Contains(error.Value)), Is.True, $"Expected error message '{error.Value}' not found.");
      }

    }

  }
