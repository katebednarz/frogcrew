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
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using misc;
using Moq.EntityFrameworkCore;

namespace TestFrogCrew.Controllers;

  [TestFixture()]
  public class CrewListControllerTests
  {
    private FrogcrewContext? _context;
    private CrewListController? _controller;
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
      _context.Games.AddRange(
        new Game
        {
          Id = 1, 
          ScheduleId = 1, 
          GameStart = TimeOnly.Parse("6:30"),
          GameDate = DateOnly.Parse("2024-11-09"),
          Venue = "Amon G Carter",
          Opponent = "OSU"
        }
      );
      
      _context.Positions.AddRange(
        new Position {PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM"},
        new Position {PositionId = 2, PositionName = "PRODUCER", PositionLocation = "CONTROL ROOM"}
      );
      
      _context.Users.AddRange(
        new ApplicationUser { Id = 1, FirstName = "Kate", LastName = "Bednarz"},
        new ApplicationUser { Id = 2, FirstName = "Aliya", LastName = "Suri"},
        new ApplicationUser { Id = 3, FirstName = "John", LastName = "Smith"}
      );

      _context.CrewedUsers.AddRange(
        new CrewedUser { UserId = 1, GameId = 1, PositionId = 1, ArrivalTime = TimeOnly.Parse("18:30") },
        new CrewedUser { UserId = 2, GameId = 1, PositionId = 2, ArrivalTime = TimeOnly.Parse("18:30") }
      );
      
      _context.Schedules.AddRange(
        new Schedule {Id = 1, Sport = "Basketball"}
      );
      
      _context.SaveChanges();
      
      _controller = new CrewListController(_context);
      _dbHelper = new DatabaseHelper(_context);
    }

    [TearDown]
    public void Teardown()
    {
      _context?.Dispose();
      _controller?.Dispose();
    }
    
    [Test()]
    public async Task FindCrewListByIdTestSuccess()
    {
      // Arrange
      var gameId = 1;
      
      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
      });

      // Verify data matches expected CrewListDTO structure
      var data = response?.Data as CrewListDTO;
      Assert.That(data, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(data?.GameId, Is.EqualTo(gameId));
        Assert.That(data?.GameStart.ToString(), Is.EqualTo(TimeOnly.Parse("6:30").ToString()));
        Assert.That(data?.GameDate.ToString(), Is.EqualTo(DateOnly.Parse("2024-11-09").ToString()));
        Assert.That(data?.Venue, Is.EqualTo("Amon G Carter"));
        Assert.That(data?.Opponent, Is.EqualTo("OSU"));
      });

      // Verify crew members in CrewListDTO
      Assert.That(data?.CrewedMembers, Has.Count.EqualTo(2));

      Assert.Multiple(() =>
      {
        // Check the first crew member
        Assert.That(data?.CrewedMembers?[0].UserId, Is.EqualTo(1));
        Assert.That(data?.CrewedMembers?[0].Position, Is.EqualTo("DIRECTOR"));
        Assert.That(data?.CrewedMembers?[1].UserId, Is.EqualTo(2));
        Assert.That(data?.CrewedMembers?[1].Position, Is.EqualTo("PRODUCER"));
      });
    }
    
    [Test()]
    public async Task FindCrewListByIdTestNotFound()
    {
      // Arrange
      var gameId = 3;
    
      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;
    
      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo($"Game with ID {gameId} not found.")); //Verify Message
      });
    }
    
    [Test()]
    public async Task ExportCrewListGameNotFound()
    {
      // Arrange
      var gameId = 3;
      
      // Act
      var result = await _controller!.FindCrewListById(gameId) as ObjectResult;
      var response = result?.Value as Result;
      
      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); //Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
        Assert.That(response?.Message, Is.EqualTo($"Game with ID {gameId} not found.")); //Verify Message
      });
    }
    
    [Test()]
    public async Task ExportCrewListSuccess()
    {
      // Arrange
      var gameId = 1;
      
      // Act
      var result = await _controller!.ExportCrewList(gameId) as FileContentResult;
      
      //Assert
      Assert.IsNotNull(result);
      Assert.That(result.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
      Assert.That(result.FileDownloadName, Is.EqualTo("CrewList.xlsx"));
    
      // Verify the Excel file structure
      using (var stream = new MemoryStream(result.FileContents))
      using (var workbook = new XLWorkbook(stream))
      {
        var worksheet = workbook.Worksheet(1);
        Assert.That(worksheet.Cell("B1").Value, Is.EqualTo("BASKETBALL"));
        Assert.That(worksheet.Cell("B2").Value, Is.EqualTo("vs"));
        Assert.That(worksheet.Cell("B3").Value, Is.EqualTo("OSU"));
        Assert.That(worksheet.Cell("B4").Value, Is.EqualTo("November 09, 2024"));
        Assert.That(worksheet.Cell("B5").Value, Is.EqualTo("6:30 AM"));
        Assert.That(worksheet.Cell("A10").Value, Is.EqualTo("DIRECTOR"));
        Assert.That(worksheet.Cell("B10").Value, Is.EqualTo("Kate Bednarz"));
        Assert.That(worksheet.Cell("C10").Value, Is.EqualTo("18:30"));
        Assert.That(worksheet.Cell("D10").Value, Is.EqualTo("CONTROL ROOM"));
      }
    }
  }
