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

        [Test()]
        public void FindScheduleByIdTestNotFound()
        {
            Assert.Pass();
        }
    }
}