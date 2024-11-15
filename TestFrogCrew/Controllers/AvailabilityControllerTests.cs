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

namespace backend.Controllers.Tests
{
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

    [Test()]
    public async Task SubmitAvailabilityTestSuccess()
    {
      // Arrange
      var request = new AvailabilityDTO
      {
        UserId = 1,
        GameId = 1,
        Open = true,
        Comment = "Coming from another game, will be 30 mins late."
      };

      var mockAvailability = new Availability
      {
        UserId = request.UserId,
        GameId = request.GameId,
        Open = request.Open,
        Comment = request.Comment
      };

      _mockContext?.Setup(c => c.Add(It.IsAny<Availability>())).Callback<Availability>(a => a.GameId = 1);
      _mockContext?.Setup(c => c.SaveChanges()).Returns(1);

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
      var availabilityDTO = response?.Data as AvailabilityDTO;
      Assert.That(availabilityDTO, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(availabilityDTO?.UserId, Is.EqualTo(request.UserId));
        Assert.That(availabilityDTO?.GameId, Is.EqualTo(request.GameId));
        Assert.That(availabilityDTO?.Open, Is.EqualTo(request.Open));
        Assert.That(availabilityDTO?.Comment, Is.EqualTo(request.Comment));
      });

      // Verify that Add and SaveChanges were called
      _mockContext?.Verify(c => c.Add(It.IsAny<Availability>()), Times.Once);
      _mockContext?.Verify(c => c.SaveChanges(), Times.Once);

    }

    [Test()]
    public async Task SubmitAvailabilityTestBadRequest()
    {
      Assert.Pass();
      // Arrange
      var request = new AvailabilityDTO
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
}