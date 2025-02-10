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
}