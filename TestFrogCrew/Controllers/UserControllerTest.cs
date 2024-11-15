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
using backend.Auth;

namespace backend.Controllers.Tests
{
    [TestFixture()]
    public class UserControllerTest
    {
        private Mock<FrogcrewContext>? _mockContext;
        private UserController? _controller;


        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _controller = new UserController(_mockContext.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _controller?.Dispose();
        }

        [Test()]
        public async Task CreateCrewMemberTestSuccess()
        {
            // Arrange
            var request = new UserDTO
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890",
                Role = "STUDENT",
                Position = new List<string> { "DIRECTOR", "PRODUCER" }
            };

            var mockUser = new User
            {
                Id = 1,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                Password = PasswordHasher.HashPassword("password")
            };

            _mockContext.Setup(c => c.Add(It.IsAny<User>())).Callback<User>(user => user.Id = mockUser.Id);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            //Act
            var result = await _controller.CreateCrewMember(request) as ObjectResult;
            var response = result?.Value as Result;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(response?.Flag); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Add Success")); //Verify Message

            //Verify Data
            var userDTO = response?.Data as UserDTO;
            Assert.IsNotNull(userDTO);
            Assert.That(userDTO?.Email, Is.EqualTo(request.Email));
            Assert.That(userDTO?.FirstName, Is.EqualTo(request.FirstName));
            Assert.That(userDTO?.LastName, Is.EqualTo(request.LastName));
            Assert.That(userDTO?.PhoneNumber, Is.EqualTo(request.PhoneNumber));
            Assert.That(userDTO?.Role, Is.EqualTo(request.Role));

            //Verify Database Saves
            _mockContext.Verify(c => c.Add(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(3)); // 1 for user, 2 for positions
        }

        [Test()]
        public async Task CreateCrewMemberTestBadRequest()
        {
            // Arrange
            var request = new UserDTO  // Empty DTO to simulate missing required fields
            {
                FirstName = null,
                LastName = null,
                Email = null,
                PhoneNumber = null,
                Role = null,
                Position = null
            };
            _controller.ModelState.AddModelError("firstName", "first name is required.");
            _controller.ModelState.AddModelError("lastName", "last name is required.");
            _controller.ModelState.AddModelError("email", "email is required.");
            _controller.ModelState.AddModelError("phoneNumber", "phone number is required.");
            _controller.ModelState.AddModelError("role", "role is required.");
            _controller.ModelState.AddModelError("position", "position is required.");

            // Act
            var result = await _controller.CreateCrewMember(request) as ObjectResult;
            var response = result?.Value as Result;

            // Expected data
            var expectedData = new Dictionary<string, string>
        {
            { "firstName", "first name is required." },
            { "lastName", "last name is required." },
            { "email", "email is required." },
            { "phoneNumber", "phone number is required." },
            { "role", "role is required." },
            { "position", "position is required." }
        };

            // Assert
            Assert.IsFalse(response?.Flag); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); //Verify Message

            // Check that the data contains the expected error messages
            Assert.IsNotNull(response?.Data);
            var errors = response.Data as List<string>;
            foreach (var error in expectedData)
            {
                Assert.IsTrue(errors.Any(e => e.Contains(error.Value)), $"Expected error message '{error.Value}' not found.");
            }
        }


        [Test()]
        public void InviteCrewMemberTestSuccess()
        {
            Assert.Pass();
        }

        [Test()]
        public void LoginTestSuccess()
        {
            Assert.Pass();
        }

        [Test()]
        public void LoginTestBadCredentials()
        {
            Assert.Pass();
        }
    }
}