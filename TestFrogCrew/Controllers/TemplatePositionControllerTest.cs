using backend.Controllers;
using backend.Models;
using backend.DTO;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using misc;
using Moq;

namespace TestFrogCrew.Controllers;

[TestFixture]
[TestOf(typeof(TemplatePositionController))]
public class TemplatePositionControllerTest
{
    private Mock<FrogcrewContext>? _mockContext;
    private TemplatePositionController _controller;
    private DatabaseHelper? _dbHelper;
    
    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<FrogcrewContext>();
        _controller = new TemplatePositionController(_mockContext.Object);
        _dbHelper = new DatabaseHelper(_mockContext.Object);
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

        // Setup IQueryable methods
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        // Setup IAsyncEnumerable method
        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return mockDbSet;
    }

    [Test]
    public async Task GetPositionsSuccess()
    {
        // Arrange
        var positions = new List<Position>
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            },
            new()
            {
                PositionId = 2,
                PositionName = "PRODUCER"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        // Act
        var result = await _controller.GetPositions() as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Find Success")); //Verify Message
        });
        
        // Verify data matches expected TradeBoardDTO structure
        var data = response?.Data as List<string>;
        
        Assert.Multiple(() =>
        {
            // Check the first crew member
            Assert.That(data?[0], Is.EqualTo("DIRECTOR"));
            Assert.That(data?[1], Is.EqualTo("PRODUCER"));
        });
        
    }

    [Test]
    public async Task AddPositionBadRequest()
    {
        // Arrange
        PositionDTO request = new()
        {
            Name = null,
        };

        Position mockPosition = new()
        {
            PositionId = 1,
            PositionName = "DIRECTOR"
        };

        var positions = new List<Position>();
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        _mockContext?.Setup(c => c.Add(It.IsAny<Position>())).Callback<Position>(position => position.PositionId = mockPosition.PositionId);
        _mockContext?.Setup(c => c.SaveChanges()).Returns(1);
        
        // Act
        var result = await _controller.AddPosition(request) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); //Verify Message
            Assert.That(response?.Data, Is.EqualTo("position is required"));
        });
    }
    
    [Test]
    public async Task AddPositionAlreadyExists()
    {
        // Arrange
        PositionDTO request = new()
        {
            Name = "DIRECTOR",
        };

        var positions = new List<Position>()
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        // Act
        var result = await _controller.AddPosition(request) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(409)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("position already exists")); //Verify Message
        });
    }
    
    [Test]
    public async Task AddPositionSuccess()
    {
        // Arrange
        PositionDTO request = new()
        {
            Name = "DIRECTOR",
        };

        Position mockPosition = new()
        {
            PositionId = 1,
            PositionName = "DIRECTOR"
        };

        var positions = new List<Position>();
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        _mockContext?.Setup(c => c.Add(It.IsAny<Position>())).Callback<Position>(position => position.PositionId = mockPosition.PositionId);
        _mockContext?.Setup(c => c.SaveChanges()).Returns(1);
        
        // Act
        var result = await _controller.AddPosition(request) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Add Success")); //Verify Message
        });
    }
    
    [Test]
    public async Task UpdatePositionNotFound()
    {
        // Arrange
        int positionId = 2;
        PositionDTO request = new()
        {
            Name = null
        };
        
        var positions = new List<Position>()
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        
        // Act
        var result = await _controller.UpdatePosition(request,positionId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(404)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Could not find position with id 2")); //Verify Message
        });
    }
    
    [Test]
    public async Task UpdatePositionBadRequest()
    {
        // Arrange
        int positionId = 1;
        PositionDTO request = new()
        {
            Name = null
        };
        
        var positions = new List<Position>()
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        _mockContext?.Setup(c => c.Positions.FindAsync(positionId))
            .ReturnsAsync(positions.FirstOrDefault(u => u.PositionId == positionId));
        _mockContext?.Setup(c => c.Add(It.IsAny<Position>())).Callback<Position>(position => position.PositionId = positionId);
        _mockContext?.Setup(c => c.SaveChanges()).Returns(1);
        
        // Act
        var result = await _controller.UpdatePosition(request,positionId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(400)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Provided arguments are invalid, see data for details.")); //Verify Message
            Assert.That(response?.Data, Is.EqualTo("position is required"));
        });
    }
    
    [Test]
    public async Task UpdatePositionAlreadyExists()
    {
        // Arrange
        int positionId = 1;
        PositionDTO request = new()
        {
            Name = "DIRECTOR"
        };
        
        var positions = new List<Position>()
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        _mockContext?.Setup(c => c.Positions.FindAsync(positionId))
            .ReturnsAsync(positions.FirstOrDefault(u => u.PositionId == positionId));
        
        // Act
        var result = await _controller.UpdatePosition(request,positionId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(409)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("position already exists")); //Verify Message
        });
    }
    
    [Test]
    public async Task UpdatePositionSuccess()
    {
        // Arrange
        int positionId = 1;
        PositionDTO request = new()
        {
            Name = "PRODUCER"
        };
        
        var positions = new List<Position>()
        {
            new()
            {
                PositionId = 1,
                PositionName = "DIRECTOR"
            }
        };
        
        var mockPositionSet = CreateMockDbSet(positions);
        _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
        _mockContext?.Setup(c => c.Positions.FindAsync(positionId))
            .ReturnsAsync(positions.FirstOrDefault(u => u.PositionId == positionId));
        
        // Act
        var result = await _controller.UpdatePosition(request,positionId) as ObjectResult;
        var response = result?.Value as Result;
        
        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); //Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); //Verify Code
            Assert.That(response?.Message, Is.EqualTo("Update Success")); //Verify Message
        });
    }
}

