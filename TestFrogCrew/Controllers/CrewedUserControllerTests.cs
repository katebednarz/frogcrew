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

namespace backend.Controllers.Tests
{
    [TestFixture()]
    public class CrewedUserControllerTests
    {
        private Mock<FrogcrewContext>? _mockContext;
        private Mock<DbSet<User>>? _mockUsersDbSet;
        private CrewedUserController? _controller;


        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _mockUsersDbSet = new Mock<DbSet<User>>();
            _controller = new CrewedUserController(_mockContext.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _controller?.Dispose();
        }

        [Test()]
        public async Task FindCrewMemberByGameAndPositionTestSuccess()
        {
            // Arrange
            int gameId = 1;
            string position = "PRODUCER";

            var users = new List<User>
        {
            new() {
                Id = 1,
                FirstName = "Billy",
                LastName = "Bob",
                Availabilities = new List<Availability>
                {
                    new() { GameId = 1, Available = 1 }
                },
                UserQualifiedPositions = new List<UserQualifiedPosition>
                {
                    new() { Position = "PRODUCER" }
                }
            },
            new() {
                Id = 2,
                FirstName = "Bob",
                LastName = "Smith",
                Availabilities = new List<Availability>
                {
                    new() { GameId = 1, Available = 1 }
                },
                UserQualifiedPositions = new List<UserQualifiedPosition>
                {
                    new() { Position = "DIRECTOR" }
                }
            },
            new() {
                Id = 3,
                FirstName = "Joe",
                LastName = "Smith",
                Availabilities = new List<Availability>
                {
                    new() { GameId = 1, Available = 1 }
                },
                UserQualifiedPositions = new List<UserQualifiedPosition>
                {
                    new() { Position = "PRODUCER" }
                }
            }
        }.AsQueryable();

            var asyncUsers = new TestAsyncEnumerable<User>(users);
            _mockUsersDbSet?.As<IAsyncEnumerable<User>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(asyncUsers.GetAsyncEnumerator);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.Provider).Returns(asyncUsers.AsQueryable().Provider);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.Expression).Returns(asyncUsers.AsQueryable().Expression);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(asyncUsers.AsQueryable().ElementType);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(asyncUsers.AsQueryable().GetEnumerator);

            _mockContext?.Setup(c => c.Users).Returns(_mockUsersDbSet!.Object);

            // Act
            var result = await _controller!.FindCrewMemberByGameAndPosition(gameId, position) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
            });

            var data = response?.Data as List<UserSimpleDTO>;
            Assert.That(data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(data?.Count, Is.EqualTo(2));
                Assert.That(data?[0].UserId, Is.EqualTo(1));
                Assert.That(data?[0].FullName, Is.EqualTo("Billy Bob"));
                Assert.That(data?[1].UserId, Is.EqualTo(3));
                Assert.That(data?[1].FullName, Is.EqualTo("Joe Smith"));
            });
        }

        [Test()]
        public async Task FindCrewMemberByGameAndPositionTestNotFound()
        {
            // Arrange
            int gameId = 1;
            string position = "Manager";

            var users = new List<User>().AsQueryable(); // No users in the database

            var asyncUsers = new TestAsyncEnumerable<User>(users);
            _mockUsersDbSet?.As<IAsyncEnumerable<User>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(asyncUsers.GetAsyncEnumerator);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.Provider).Returns(asyncUsers.AsQueryable().Provider);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.Expression).Returns(asyncUsers.AsQueryable().Expression);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(asyncUsers.AsQueryable().ElementType);
            _mockUsersDbSet?.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(asyncUsers.AsQueryable().GetEnumerator);

            _mockContext?.Setup(c => c.Users).Returns(_mockUsersDbSet!.Object);

            // Act
            var result = await _controller!.FindCrewMemberByGameAndPosition(gameId, position) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo($"No matching crew members available for {position}")); //Verify Message
            });

        }
    }
}