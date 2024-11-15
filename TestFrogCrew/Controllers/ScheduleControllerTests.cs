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
        public async Task CreateGameScheduleTestSuccess()
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
            var result = await _controller!.CreateGameSchedule(request) as ObjectResult;
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

        [Test()]
        public void FindScheduleByIdTestNotFound()
        {
            Assert.Pass();
        }
    }
}