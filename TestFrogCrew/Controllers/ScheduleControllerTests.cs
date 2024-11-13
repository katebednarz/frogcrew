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
            throw new NotImplementedException();
        }

        [Test()]
        public void CreateGameScheduleTestBadRequest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void CreateGameScheduleGamesTestSuccess()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void CreateGameScheduleGamesTestBadRequest()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void CreateGameScheduleGamesTestNotFound()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void FindScheduleByIdTestSuccess()
        {
            throw new NotImplementedException();
        }

        [Test()]
        public void FindScheduleByIdTestNotFound()
        {
            throw new NotImplementedException();
        }
    }
}