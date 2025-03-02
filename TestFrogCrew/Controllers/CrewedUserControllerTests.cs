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
using backend.Utils;
using Microsoft.EntityFrameworkCore;
using misc;
using Moq.EntityFrameworkCore;

namespace TestFrogCrew.Controllers;

    [TestFixture()]
    public class CrewedUserControllerTests
    {
        private FrogcrewContext? _context;
        private CrewedUserController? _controller;
        private DatabaseHelper? _dbHelper;


        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FrogcrewContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            
            _context = new FrogcrewContext(options);
      
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            
            // Adding Test Data
            _context.Users.AddRange(
                new ApplicationUser { Id = 1, FirstName = "Kate", LastName = "Bednarz"},
                new ApplicationUser { Id = 2, FirstName = "Aliya", LastName = "Suri"},
                new ApplicationUser { Id = 3, FirstName = "John", LastName = "Smith"}
            );
      
            _context.Games.AddRange(
                new Game { Id = 1, ScheduleId = 1},
                new Game { Id = 2, ScheduleId = 1}
            );
      
            _context.Schedules.AddRange(
                new Schedule {Id = 1, Sport = "Basketball"}
            );
            
            _context.Positions.AddRange(
                new Position {PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM"}
            );
            
            _context.Availabilities.AddRange(
                new Availability { UserId = 1, GameId = 1, Available = 1},
                new Availability { UserId = 3, GameId = 1, Available = 1}
                
            );
            
            _context.UserQualifiedPositions.AddRange(
                new UserQualifiedPosition {PositionId = 1, UserId = 1},
                new UserQualifiedPosition {PositionId = 1, UserId = 3}
            );
      
            _context.SaveChanges();
            
            _controller = new CrewedUserController(_context);
            _dbHelper = new DatabaseHelper(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test()]
        public async Task FindCrewMemberByGameAndPositionTestSuccess()
        {
            // Arrange
            var gameId = 1;
            var positionName = "DIRECTOR";
            
            // Act
            var result = await _controller!.FindCrewMemberByGameAndPosition(gameId, positionName) as ObjectResult;
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
                Assert.That(data?[0].FullName, Is.EqualTo("Kate Bednarz"));
                Assert.That(data?[1].UserId, Is.EqualTo(3));
                Assert.That(data?[1].FullName, Is.EqualTo("John Smith"));
            });
        }

        [Test()]
        public async Task FindCrewMemberByGameAndPositionTestNotFound()
        {
            // Arrange
            int gameId = 1;
            int positionId = 1;
            string positionName = "PRODUCER";
            // Act
            var result = await _controller!.FindCrewMemberByGameAndPosition(gameId, positionName) as ObjectResult;
            var response = result?.Value as Result;
        
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(response?.Flag, Is.False); //Verify Flag
                Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
                Assert.That(response?.Message, Is.EqualTo($"No matching crew members available for {positionName}")); //Verify Message
            });
        
        }
    }
