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

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            #pragma warning disable CS8605, CS8602
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => queryable.FirstOrDefault(d => (int)d.GetType().GetProperty("Id").GetValue(d) == (int)ids[0]));
            #pragma warning restore CS8605, CS8602
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
            _mockContext?.Setup(c => c.Games.FindAsync(gameId)).ReturnsAsync(null as Game);

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
            var scheduleId = 1;
            var schedule = new Schedule { Id = scheduleId };
            var games = new List<Game>
            {
                new() { Id = 1, ScheduleId = scheduleId, Opponent = "Team A" },
                new() { Id = 2, ScheduleId = scheduleId, Opponent = "Team B" }
            };

            var mockScheduleSet = CreateMockDbSet(new List<Schedule> { schedule });
            var mockGameSet = CreateMockDbSet(games);

            _mockContext?.Setup(c => c.Schedules).Returns(mockScheduleSet.Object);
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

            var data = response?.Data as List<GameDTO>;
            Assert.That(data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(data?.Count, Is.EqualTo(2));
                Assert.That(data?[0].GameId, Is.EqualTo(1));
                Assert.That(data?[0].ScheduleId, Is.EqualTo(scheduleId));
                Assert.That(data?[0].Opponent, Is.EqualTo("Team A"));
                Assert.That(data?[1].GameId, Is.EqualTo(2));
                Assert.That(data?[1].ScheduleId, Is.EqualTo(scheduleId));
                Assert.That(data?[1].Opponent, Is.EqualTo("Team B"));
            });

            // Verify FindAsync was called once
            mockScheduleSet.Verify(m => m.FindAsync(It.IsAny<object[]>()), Times.Once);
        }

        [Test()]
        public async Task FindGamesByScheduleIdTestNotFound()
        {
            // Arrange
            var scheduleId = 1;
            var mockScheduleSet = CreateMockDbSet(new List<Schedule>());
            _mockContext?.Setup(c => c.Schedules).Returns(mockScheduleSet.Object);

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

            // Verify FindAsync was called once
            mockScheduleSet.Verify(m => m.FindAsync(It.IsAny<object[]>()), Times.Once);

        }
        
        [Test]
        public async Task UpdateGameNotFound()
        {
            // Arrange
            int gameId = 2;
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
            
            var mockScheduleSet = CreateMockDbSet(games);
            _mockContext?.Setup(c => c.Games).Returns(mockScheduleSet.Object);
            
            // Act
            var result = await _controller!.UpdateGameById(gameId, request) as ObjectResult;
            var response = result?.Value as Result;
            
            // Assert 
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); // Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); // Verify Code
                Assert.That(response?.Message, Is.EqualTo("Could not find game with id 2")); // Verify Message
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
            
            var games = new List<Game>()
            {
                new()
                {
                    Id = 1,
                    ScheduleId = 1,
                    GameDate = new DateOnly(2024, 11, 06),
                    Venue = "Carter",
                    Opponent = "Baylor"
                }
            };
            
            var mockPositionSet = CreateMockDbSet(games);
            _mockContext?.Setup(c => c.Games).Returns(mockPositionSet.Object);
            _mockContext?.Setup(c => c.Games.FindAsync(gameId))
                .ReturnsAsync(games.FirstOrDefault(u => u.Id == gameId));
            _mockContext?.Setup(c => c.Add(It.IsAny<Game>())).Callback<Game>(game => game.Id = gameId);
            _mockContext?.Setup(c => c.SaveChanges()).Returns(1);
            
            // Arrange
            _controller.ModelState.AddModelError(nameof(GameDTO.GameDate), "Game date is required.");
            _controller.ModelState.AddModelError(nameof(GameDTO.Venue), "Venue is required.");
            _controller.ModelState.AddModelError(nameof(GameDTO.Opponent), "Opponent is required.");
            
            
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
            
            var games = new List<Game>()
            {
                new()
                {
                    Id = 1,
                    ScheduleId = 1,
                    GameDate = new DateOnly(2024, 11, 06),
                    Venue = "Carter",
                    Opponent = "Baylor"
                }
            };
            
            var mockScheduleSet = CreateMockDbSet(games);
            _mockContext?.Setup(c => c.Games).Returns(mockScheduleSet.Object);
            _mockContext?.Setup(c => c.Games.FindAsync(gameId))
                .ReturnsAsync(games.FirstOrDefault(u => u.Id == gameId));
            
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
                GameDate = new DateOnly(2024, 11, 06),
                Venue = "Carter",
                Opponent = "Baylor"
            };
            
            var games = new List<Game>()
            {
                new()
                {
                    Id = 1,
                    ScheduleId = 1,
                    GameDate = new DateOnly(2024, 12, 07),
                    Venue = "New Venue",
                    Opponent = "New Opponent"
                }
            };
            
            var mockPositionSet = CreateMockDbSet(games);
            _mockContext?.Setup(c => c.Games).Returns(mockPositionSet.Object);
            _mockContext?.Setup(c => c.Games.FindAsync(gameId))
                .ReturnsAsync(games.FirstOrDefault(u => u.Id == gameId));
            
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
