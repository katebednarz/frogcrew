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
        private CrewedUserController? _controller;


        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<FrogcrewContext>();
            _controller = new CrewedUserController(_mockContext.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _controller?.Dispose();
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            // Set up IQueryable
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Set up IAsyncEnumerable
            mockDbSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            return mockDbSet;
        }


        [Test()]
        public async Task FindCrewMemberByGameAndPositionTestSuccess()
        {
            // Arrange
            var gameId = 1;
            var position = "PRODUCER";

            var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Billy",
            Availabilities = new List<Availability>
            {
                new Availability { GameId = gameId, Open = true }
            },
            UserQualifiedPositions = new List<UserQualifiedPosition>
            {
                new UserQualifiedPosition { Position = "PRODUCER" }
            }
        },
        new User
        {
            Id = 2,
            FirstName = "Bob",
            Availabilities = new List<Availability>
            {
                new Availability { GameId = gameId, Open = true }
            },
            UserQualifiedPositions = new List<UserQualifiedPosition>
            {
                new UserQualifiedPosition { Position = "DIRECTOR" }
            }
        },
        new User
        {
            Id = 3,
            FirstName = "Joe",
            Availabilities = new List<Availability>
            {
                new Availability { GameId = gameId, Open = false }
            },
            UserQualifiedPositions = new List<UserQualifiedPosition>
            {
                new UserQualifiedPosition { Position = "PRODUCER" }
            }
        }
    };

            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);

            // Act
            var result = await _controller.FindCrewMemberByGameAndPosition(gameId, position) as ObjectResult;
            var response = result?.Value as Result;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.True);
                Assert.That(response?.Code, Is.EqualTo(200));
                Assert.That(response?.Message, Is.EqualTo("Find Success"));
            });

            var availableQualifiedUsers = response?.Data as List<UserSimpleDTO>;
            Assert.That(availableQualifiedUsers, Is.Not.Null);
            Assert.That(availableQualifiedUsers, Has.Count.EqualTo(1));

            Assert.That(availableQualifiedUsers[0].FullName, Is.EqualTo("Billy"));

        }

        [Test()]
        public void FindCrewMemberByGameAndPositionTestNotFound()
        {
            Assert.Pass();
        }
    }
}