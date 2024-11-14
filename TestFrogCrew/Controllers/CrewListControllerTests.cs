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
    public class CrewListControllerTests
    {
        private Mock<FrogcrewContext>? _mockContext;
        private CrewListController? _controller;


        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _controller = new CrewListController(_mockContext.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _controller?.Dispose();
        }

        [Test()]
        public void FindCrewListByIdTestSuccess()
        {
            Assert.Pass();
        }

        [Test()]
        public void FindCrewListByIdTestNotFound()
        {
            Assert.Pass();
        }
    }
}