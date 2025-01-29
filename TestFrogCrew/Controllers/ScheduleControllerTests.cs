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
using Moq.EntityFrameworkCore;

namespace backend.Controllers.Tests
{
    [TestFixture()]
    public class ScheduleControllerTests
    {
        private Mock<FrogcrewContext>? _mockContext;
        private ScheduleController? _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _controller = new ScheduleController(_mockContext.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _controller?.Dispose();
        }

        [Test()]
        public void CreateGameScheduleTestSuccess()
        {
            // Arrange
            var request = new GameScheduleDTO
            {
                Sport = "Women's Basketball",
                Season = "2024"
            };

            var mockSchedule = new Schedule
            {
                Id = 1,
                Sport = request.Sport,
                Season = request.Season
            };

            _mockContext?.Setup(c => c.Add(It.IsAny<Schedule>())).Callback<Schedule>(schedule => schedule.Id = mockSchedule.Id);
            _mockContext?.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = _controller!.CreateGameSchedule(request) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() => {
                Assert.IsNotNull(result);
                Assert.IsTrue(response?.Flag); // Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
                Assert.That(response?.Message, Is.EqualTo("Add Success")); // Verify Message
            });

            // Verify Data
            var gameScheduleDTO = response?.Data as GameScheduleDTO;
            Assert.Multiple(() => {
                Assert.IsNotNull(gameScheduleDTO);
                Assert.That(gameScheduleDTO?.Sport, Is.EqualTo(request.Sport));
                Assert.That(gameScheduleDTO?.Season, Is.EqualTo(request.Season));
            });
            // Verify Database Saves
            _mockContext?.Verify(c => c.Add(It.IsAny<Schedule>()), Times.Once);
            _mockContext?.Verify(c => c.SaveChanges(), Times.Exactly(1)); // 1 for schedule
        }

        [Test()]
        public void CreateGameScheduleTestBadRequest()
        {
            // Arrange
            var request = new GameScheduleDTO  // Empty DTO to simulate missing required fields
            {
                Sport = null,
                Season = null
            };
            _controller!.ModelState.AddModelError("Sport", "Sport is required.");
            _controller.ModelState.AddModelError("Season", "Season is required.");

            // Act
            var result = _controller.CreateGameSchedule(request) as ObjectResult;
            var response = result?.Value as Result;

            // Expected data
            var expectedData = new Dictionary<string, string>
            {
                { "Sport", "Sport is required." },
                { "Season", "Season is required." }
            };

            // Assert
            Assert.IsFalse(response?.Flag); // Verify Flag
            Assert.That(response?.Code, Is.EqualTo(400)); // Verify Code
            Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); // Verify Message

            // Check that the data contains the expected error messages
            Assert.IsNotNull(response?.Data);
            var errors = response!.Data as List<string>;
            foreach (var error in expectedData)
            {
                Assert.IsTrue(errors!.Any(e => e.Contains(error.Value)), $"Expected error message '{error.Value}' not found.");
            }
        }

        [Test()]
        public async Task CreateGameScheduleGamesTestSuccess()
        {
            // Arrange
            var scheduleId = 1;
            var gameSchedule = new Schedule { Id = scheduleId, Sport = "Men's Basketball", Season = "2024" };
            
            var games = new List<GameDTO>
            {
                new() { GameDate = DateOnly.FromDateTime(DateTime.UtcNow), Venue = "Stadium A", Opponent = "Team X" },
                new() { GameDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), Venue = "Stadium B", Opponent = "Team Y" }
            };

            _mockContext?.Setup(c => c.Schedules.FindAsync(scheduleId)).ReturnsAsync(gameSchedule);
            _mockContext?.Setup(c => c.Games.Add(It.IsAny<Game>()));
            _mockContext?.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = await _controller!.CreateGameScheduleGames(scheduleId, games) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True);
                Assert.That(response?.Code, Is.EqualTo(200));
                Assert.That(response?.Message, Is.EqualTo("Add Success"));
            });

            var responseData = response?.Data as List<GameDTO>;
            Assert.Multiple(() =>
            {
                Assert.That(responseData, Is.Not.Null);
                Assert.That(responseData?.Count, Is.EqualTo(2));
                Assert.That(responseData?[0].ScheduleId, Is.EqualTo(scheduleId));
                Assert.That(responseData?[1].ScheduleId, Is.EqualTo(scheduleId));
            });

            _mockContext?.Verify(c => c.Games.Add(It.IsAny<Game>()), Times.Exactly(2));
            _mockContext?.Verify(c => c.SaveChanges(), Times.Exactly(2));
        }

        [Test()]
        public async Task CreateGameScheduleGamesTestBadRequest()
        {
            // Arrange
            var scheduleId = 99; // Non-existent schedule ID
            var games = new List<GameDTO>
            {
                new() { GameDate = DateOnly.FromDateTime(DateTime.UtcNow), Venue = "Stadium A", Opponent = "Team X" }
            };

            _mockContext?.Setup(c => c.Schedules.FindAsync(scheduleId)).ReturnsAsync((Schedule?)null);

            // Act
            var result = await _controller!.CreateGameScheduleGames(scheduleId, games) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); // Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); // Verify Code
                Assert.That(response?.Message, Is.EqualTo($"Could not find schedule with ID {scheduleId}.")); // Verify Message
            });

            // Ensure no games were added or saved
            _mockContext?.Verify(c => c.Games.Add(It.IsAny<Game>()), Times.Never);
            _mockContext?.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Test()]
        public void CreateGameScheduleGamesTestNotFound()
        {
            Assert.Pass();
        }

        [Test()]
        public async Task FindScheduleByIdTestSuccess()
        {
            // Arrange
            var scheduleId = 1;
            var schedule = new Schedule
            {
                Id = scheduleId,
                Sport = "Men's Soccer",
                Season = "2024"
            };

            _mockContext?.Setup(c => c.Schedules.FindAsync(scheduleId))
                        .ReturnsAsync(schedule);

            // Act
            var result = await _controller!.FindScheduleById(scheduleId) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); // verify flag
                Assert.That(response?.Code, Is.EqualTo(200)); // verify code
                Assert.That(response?.Message, Is.EqualTo("Find Success")); // verify message
            });

            var gameScheduleDTO = response?.Data as GameScheduleDTO;
            Assert.Multiple(() =>
            {
                Assert.That(gameScheduleDTO, Is.Not.Null);
                Assert.That(gameScheduleDTO?.Id, Is.EqualTo(scheduleId));
                Assert.That(gameScheduleDTO?.Sport, Is.EqualTo("Men's Soccer"));
                Assert.That(gameScheduleDTO?.Season, Is.EqualTo("2024"));
            });
        }

        [Test()]
        public async Task FindScheduleByIdTestNotFound()
        {
            // Arrange
            var scheduleId = 99; // an ID that doesn't exist
            _mockContext?.Setup(c => c.Schedules.FindAsync(scheduleId))
                        .ReturnsAsync((Schedule?)null);

            // Act
            var result = await _controller!.FindScheduleById(scheduleId) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); // verify flag
                Assert.That(response?.Code, Is.EqualTo(404)); // verify code
                Assert.That(response?.Message, Is.EqualTo($"Could not find schedule with Id {scheduleId}.")); // verify message
            });
        }

        [Test()]
        public async Task FindGameSchedulesBySeasonTestSuccess()
        {
            // Arrange
            var season = "2024";
            var schedules = new List<Schedule>
            {
                new() { Id = 1, Season = "2024" },
                new() { Id = 2, Season = "2024" }
            };

            _mockContext?.Setup(c => c.Schedules).ReturnsDbSet(schedules);

            // Act
            var result = await _controller!.FindGameSchedulesBySeason(season) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
            });

            var data = response?.Data as List<GameScheduleDTO>;
            Assert.That(data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(data?.Count, Is.EqualTo(2));
                Assert.That(data?[0].Id, Is.EqualTo(1));
                Assert.That(data?[0].Season, Is.EqualTo("2024"));
                Assert.That(data?[1].Id, Is.EqualTo(2));
                Assert.That(data?[1].Season, Is.EqualTo("2024"));
            });
        }

        [Test()]
        public async Task FindGameSchedulesBySeasonTestNotFound()
        {
            // Arrange
            var season = "2024";
            var schedules = new List<Schedule>();

            _mockContext?.Setup(c => c.Schedules).ReturnsDbSet(schedules);

            // Act
            var result = await _controller!.FindGameSchedulesBySeason(season) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Could not find any schedules for season 2024.")); //Verify Message
            });
        }
    }
}