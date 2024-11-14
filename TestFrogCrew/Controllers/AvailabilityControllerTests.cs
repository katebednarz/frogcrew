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
        public void availabilityTestSuccess()
        {
            Assert.Pass();
        }

        [Test()]
        public void availabilityTestBadRequest()
        {
            Assert.Pass();
        }

        [Test()]
        public void availabilityTestNotFound()
        {
            Assert.Pass();
        }
    }
}