using System.Text;
using Moq;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using backend.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq.EntityFrameworkCore;

namespace backend.Controllers.Tests
{
    [TestFixture()]
    public class UserControllerTest
    {
        private Mock<FrogcrewContext>? _mockContext;
        private IConfiguration _config;
        private Mock<ISession>? _mockSession;
        private UserController? _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _mockSession = new Mock<ISession>();

            var httpContext = new DefaultHttpContext
            {
                Session = _mockSession.Object
            };
            
            _controller = new UserController(_mockContext.Object, _config)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

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
                Position = ["DIRECTOR", "PRODUCER"]
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

            _mockContext?.Setup(c => c.Add(It.IsAny<User>())).Callback<User>(user => user.Id = mockUser.Id);
            _mockContext?.Setup(c => c.SaveChanges()).Returns(1);

            //Act
            var result = await _controller!.CreateCrewMember(request) as ObjectResult;
            var response = result?.Value as Result;
            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Add Success")); //Verify Message
            });

            //Verify Data
            var userDTO = response?.Data as UserDTO;
            Assert.That(userDTO, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(userDTO?.Email, Is.EqualTo(request.Email));
                Assert.That(userDTO?.FirstName, Is.EqualTo(request.FirstName));
                Assert.That(userDTO?.LastName, Is.EqualTo(request.LastName));
                Assert.That(userDTO?.PhoneNumber, Is.EqualTo(request.PhoneNumber));
                Assert.That(userDTO?.Role, Is.EqualTo(request.Role));
            });

            //Verify Database Saves
            _mockContext?.Verify(c => c.Add(It.IsAny<User>()), Times.Once);
            _mockContext?.Verify(c => c.SaveChanges(), Times.Exactly(3)); // 1 for user, 2 for positions
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
            _controller?.ModelState.AddModelError("firstName", "first name is required.");
            _controller?.ModelState.AddModelError("lastName", "last name is required.");
            _controller?.ModelState.AddModelError("email", "email is required.");
            _controller?.ModelState.AddModelError("phoneNumber", "phone number is required.");
            _controller?.ModelState.AddModelError("role", "role is required.");
            _controller?.ModelState.AddModelError("position", "position is required.");

            // Act
            var result = await _controller!.CreateCrewMember(request) as ObjectResult;
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

        [Test()]
        public async Task InviteCrewMemberTestSuccess()
        {
            // Arrange
            var request = new EmailDTO
            {
                Emails = ["test1@example.com", "test2@example.com"]
            };

            // Act
            var result = await _controller!.InviteCrewMember(request) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Invite success")); //Verify Message
            });
            CollectionAssert.AreEquivalent(request.Emails, response?.Data as List<string>);
        }

        [Test()]
        public async Task InviteCrewMemberTestBadRequest()
        {
            // Arrange
            var request = new EmailDTO // Empty or invalid DTO to simulate model validation failure
            {
                Emails = null
            };

            _controller?.ModelState.AddModelError("Emails", "Emails are required.");

            // Act
            var result = await _controller!.InviteCrewMember(request) as ObjectResult;
            var response = result?.Value as Result;

            // Expected data
            var expectedErrors = new List<string> { "Emails are required." };

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False);
                Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); //Verify Message
            });
            // Check that the data contains the expected error message
            CollectionAssert.AreEquivalent(expectedErrors, response?.Data as List<string>);
        }

        [Test()]
        public void LoginTestSuccess()
        {
            // Arrange
            var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user@example.com:correctpassword"));
            _controller!.ControllerContext.HttpContext.Request.Headers.Authorization = authHeader;

            _mockContext?.Setup(c => c.Users).ReturnsDbSet(new List<User>
        {
            new() { Id = 1, Email = "user@example.com", Password = PasswordHasher.HashPassword("correctpassword"), Role = "Admin" }
        });

            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as Result;
            Assert.Multiple(() =>
            {
                Assert.That(response?.Flag, Is.True);
                Assert.That(response?.Code, Is.EqualTo(200));
                Assert.That(response?.Message, Is.EqualTo("Login successful"));
                Assert.That(response?.Data, Is.Not.Null); // Token
            });
    }

        [Test()]
        public void LoginTestBadCredentials()
        {
            // Arrange
            var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user@example.com:wrongpassword"));
            _controller!.ControllerContext.HttpContext.Request.Headers.Authorization = authHeader;

            _mockContext?.Setup(c => c.Users).ReturnsDbSet(new List<User>
        {
            new() { Email = "user@example.com", Password = PasswordHasher.HashPassword("correctpassword") }
        });

            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.StatusCode, Is.EqualTo(401));
            Assert.That(((Result)unauthorizedResult.Value!).Message, Is.EqualTo("Invalid email or password"));
        }

        [Test()]
        public void LoginTestNoAuthorizationHeader()
        {
            // Arrange
            _controller?.ControllerContext.HttpContext.Request.Headers.Clear();

            // Act
            var result = _controller!.Login();

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.StatusCode, Is.EqualTo(401));
            Assert.That(((Result)unauthorizedResult.Value!).Message, Is.EqualTo("Missing Authorization Header"));
        }

        [Test()]
        public void LoginTestInvalidAuthorizationHeader()
        {
            // Arrange
            _controller!.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer invalid_token";

            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.StatusCode, Is.EqualTo(401));
            Assert.That(((Result)unauthorizedResult.Value!).Message, Is.EqualTo("Invalid Authorization Header"));

        }
    }
}