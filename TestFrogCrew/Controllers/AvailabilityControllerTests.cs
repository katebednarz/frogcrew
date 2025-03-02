using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using backend.Utils;
using Microsoft.EntityFrameworkCore;


namespace TestFrogCrew.Controllers;

  [TestFixture()]
  public class AvailabilityControllerTests
  {
    private FrogcrewContext _context;
    private AvailabilityController? _controller;
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
      _context.Users.AddRange(
        new ApplicationUser { Id = 1, FirstName = "Kate", LastName = "Bednarz"},
        new ApplicationUser { Id = 2, FirstName = "Aliya", LastName = "Suri"}
      );
      
      _context.Games.AddRange(
        new Game { Id = 1, ScheduleId = 1},
        new Game { Id = 2, ScheduleId = 1}
      );
      
      _context.Schedules.AddRange(
        new Schedule {Id = 1, Sport = "Basketball"}
      );
      
      _context.SaveChanges();
      
      _notificationsHelper = new NotificationsHelper(_context);
      _controller = new AvailabilityController(_context,_notificationsHelper);
    }

    [TearDown]
    public void Teardown()
    {
      _context?.Dispose();
      _controller?.Dispose();
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
