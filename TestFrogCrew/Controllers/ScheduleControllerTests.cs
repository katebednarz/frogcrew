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
            Assert.Pass();
        }

        [Test()]
        public void CreateGameScheduleTestBadRequest()
        {
            Assert.Pass();
        }

        [Test()]
        public void CreateGameScheduleGamesTestSuccess()
        {
            Assert.Pass();
        }

        [Test()]
        public void CreateGameScheduleGamesTestBadRequest()
        {
            Assert.Pass();
        }

        [Test()]
        public void CreateGameScheduleGamesTestNotFound()
        {
            Assert.Pass();
        }

        [Test()]
        public void FindScheduleByIdTestSuccess()
        {
            Assert.Pass();
        }

        [Test]
        public void FindScheduleByIdTestNotFound()
        {
            Assert.Pass();
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