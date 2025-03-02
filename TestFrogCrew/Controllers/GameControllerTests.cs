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

namespace TestFrogCrew.Controllers;

    [TestFixture()]
    public class GameControllerTests
    {
        private FrogcrewContext? _context;
        private GameController? _controller;

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
            _context.Games.AddRange(
                new Game { Id = 1, ScheduleId = 1, Opponent = "Team A"},
                new Game { Id = 2, ScheduleId = 1, Opponent = "Team B"},
                new Game
                    {
                        Id = 3,
                        ScheduleId = 1,
                        GameDate = new DateOnly(2024, 11, 06),
                        Venue = "Carter",
                        Opponent = "Baylor"
                    }
            );
            _context.Schedules.AddRange(
                new Schedule {Id = 1, Sport = "Basketball"}
            );
            
            _context.SaveChanges();
            
            _controller = new GameController(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test()]
        public async Task FindGameByIdTestSuccess()
        {
            // Arrange
            var gameId = 1;

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
            Assert.That(gameDTO?.GameId, Is.EqualTo(gameDTO.GameId));
        }

        [Test()]
        public async Task FindGameByIdTestNotFound()
        {
            // Arrange
            var gameId = 4;
            
        
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
        }
        
        [Test()]
        public async Task FindGamesByScheduleIdTestSuccess()
        {
            // Arrange
            var scheduleId = 1;
        
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
        
            var data = response?.Data as List<GameDTO>;
            Assert.That(data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(data?.Count, Is.EqualTo(3));
                Assert.That(data?[0].GameId, Is.EqualTo(1));
                Assert.That(data?[0].ScheduleId, Is.EqualTo(scheduleId));
                Assert.That(data?[0].Opponent, Is.EqualTo("Team A"));
                Assert.That(data?[1].GameId, Is.EqualTo(2));
                Assert.That(data?[1].ScheduleId, Is.EqualTo(scheduleId));
                Assert.That(data?[1].Opponent, Is.EqualTo("Team B"));
            });
        }
        
        [Test()]
        public async Task FindGamesByScheduleIdTestNotFound()
        {
            // Arrange
            var scheduleId = 2;
            // Act
            var result = await _controller!.FindGamesByScheduleId(scheduleId) as ObjectResult;
            var response = result?.Value as Result;
        
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); // Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); // Verify Code
                Assert.That(response?.Message, Is.EqualTo($"Could not find schedule with Id {scheduleId}.")); // Verify Message
            });
        }
        
        [Test]
        public async Task UpdateGameNotFound()
        {
            // Arrange
            int gameId = 5;
            GameDTO request = new()
            {
                ScheduleId = 1,
                GameDate = new DateOnly(2024, 11, 06),
                Venue = "Carter",
                Opponent = "Baylor"
            };
            
            var games = new List<Game>()
            {
                new()
                {
                    ScheduleId = 1,
                    GameDate = new DateOnly(2024, 11, 06),
                    Venue = "Carter",
                    Opponent = "Baylor"
                }
            };
            
            // Act
            var result = await _controller!.UpdateGameById(gameId, request) as ObjectResult;
            var response = result?.Value as Result;
            
            // Assert 
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); // Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); // Verify Code
                Assert.That(response?.Message, Is.EqualTo("Could not find game with id 5")); // Verify Message
            });
        }
        
        [Test]
        public async Task UpdateGameBadRequest()
        {
            // Arrange
            int gameId = 1;
            GameDTO request = new()
            {
                GameDate = null,
                Venue = null,
                Opponent = null
            };
            
            // Arrange
            _controller?.ModelState.AddModelError(nameof(GameDTO.GameDate), "Game date is required.");
            _controller?.ModelState.AddModelError(nameof(GameDTO.Venue), "Venue is required.");
            _controller?.ModelState.AddModelError(nameof(GameDTO.Opponent), "Opponent is required.");
            
            
            // Act
            var result = await _controller!.UpdateGameById(gameId, request) as ObjectResult;
            var response = result?.Value as Result;
            
            // Expected data
            var expectedData = new Dictionary<string, string>
            {
                { "gameDate", "Game date is required." },
                { "venue", "Venue is required." },
                { "opponent", "Opponent is required." },
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
        
        [Test]
        public async Task UpdateGameAlreadyExists()
        {
            // Arrange
            int gameId = 1;
            GameDTO request = new()
            {
                GameDate = new DateOnly(2024, 11, 06),
                ScheduleId = 1,
                Venue = "Carter",
                Opponent = "Baylor"
            };
            
            // Act
            var result = await _controller!.UpdateGameById(gameId, request) as ObjectResult;
            var response = result?.Value as Result;
            
            // Assert 
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(409)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Game already exists")); //Verify Message
            });
        }
        
        [Test]
        public async Task UpdateGameSuccess()
        {
            // Arrange
            int gameId = 1;
            GameDTO request = new()
            {
                ScheduleId = 1,
                GameDate = new DateOnly(2024, 12, 06),
                Venue = "Carter",
                Opponent = "Texas"
            };
            
            // Act
            var result = await _controller.UpdateGameById(gameId, request) as ObjectResult;
            var response = result?.Value as Result;
            
            // Assert 
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Update Success")); //Verify Message
            });
        }
    }
