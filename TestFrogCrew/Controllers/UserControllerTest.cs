using System.Text;
using Moq;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using backend.Auth;
using backend.Controllers;
using backend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using misc;
using Moq.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TestFrogCrew.Controllers
{
  [TestFixture()]
  public class UserControllerTest
  {
    private Mock<FrogcrewContext>? _mockContext;
    private IConfiguration? _config;
    private Mock<ISession>? _mockSession;
    private UserController? _controller;
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private DatabaseHelper? _dbHelper;

    [SetUp]
    public void Setup()
    {
      _mockContext = new Mock<FrogcrewContext>();
      _dbHelper = new DatabaseHelper(_mockContext.Object);
      _userManagerMock = new Mock<UserManager<ApplicationUser>>(
        new Mock<IUserStore<ApplicationUser>>().Object,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
      );
      var _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
      var _userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
      _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
        _userManagerMock.Object,
        _httpContextAccessorMock.Object,
        _userClaimsPrincipalFactoryMock.Object,
        null,
        null,
        null,
        null
      );
      _config = new ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .Build();
      _mockSession = new Mock<ISession>();

      var httpContext = new DefaultHttpContext
      {
        Session = _mockSession.Object
      };

      _controller = new UserController(_userManagerMock.Object, _signInManagerMock.Object, _mockContext.Object, _config)
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

    [Test()]
    public async Task CreateCrewMemberTestSuccess()
    {
      // Arrange
      int positionId = 1;
      string positionName = "DIRECTOR";
      
      _mockContext?.Setup(c => c.Positions)
        .ReturnsDbSet(new List<Position>
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
        });

      _mockContext?.Setup(c => c.Invitations)
        .ReturnsDbSet(new List<Invitation> { new Invitation {Token = "token"} });
      
      var request = new UserDTO
      {
        Email = "test@example.com",
        FirstName = "John",
        LastName = "Doe",
        PhoneNumber = "1234567890",
        Role = "STUDENT",
        Position = ["DIRECTOR", "PRODUCER"]
      };

      _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

      _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), request.Role))
        .ReturnsAsync(IdentityResult.Success);

      var dbSetMock = new Mock<DbSet<UserQualifiedPosition>>();
      _mockContext.Setup(db => db.Set<UserQualifiedPosition>()).Returns(dbSetMock.Object);

      //Act
      var result = await _controller!.CreateCrewMember(request,"token") as ObjectResult;
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
      _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
      _mockContext?.Verify(c => c.SaveChanges(), Times.Exactly(2)); //2 for positions
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
      var result = await _controller!.CreateCrewMember(request,"token") as ObjectResult;
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
    public async Task ValidateInvitationTokenTestSuccess()
    {
        // Arrange
        string token = "valid-token";
        var invitation = new Invitation { Token = token };

        _mockContext?.Setup(c => c.Invitations).ReturnsDbSet(new List<Invitation> { invitation });

        // Act
        var result = await _controller!.ValidateInvitation(token) as ObjectResult;
        var response = result?.Value as Result;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.True); // Verify Flag
            Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
            Assert.That(response?.Message, Is.EqualTo("Invitation valid")); // Verify Message
        });

        var data = response?.Data as dynamic;
        Assert.That(data, Is.Not.Null);
    }

    [Test()]
    public async Task ValidateInvitationTokenTestBadRequest()
    {
        // Arrange
        string token = null; // simulating missing token

        // Act
        var result = await _controller!.ValidateInvitation(token) as ObjectResult;
        var response = result?.Value as Result;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(response?.Flag, Is.False); // verify flag
            Assert.That(response?.Code, Is.EqualTo(400)); // verify code
            Assert.That(response?.Message, Is.EqualTo("Token is required")); // verify message
        });
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

    [Test]
    public async Task LoginSuccessTest()
    {
      // Arrange
      var email = "test@example.com";
      var password = "Password!1";
      var user = new ApplicationUser
      {
        Id = 1,
        UserName = email,
        Email = email,
        FirstName = "John",
        LastName = "Doe",
      };

      _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
      _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, password, false)).ReturnsAsync(SignInResult.Success);
      _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "STUDENT" });

      // Act
      var result = await _controller!.Login(email, password) as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); // Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
        Assert.That(response?.Message, Is.EqualTo("Login successful")); // Verify Message
      });

      var authDTO = response?.Data as AuthDTO;
      Assert.That(authDTO, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(authDTO?.UserId, Is.EqualTo(user.Id));
        Assert.That(authDTO?.Role, Is.EqualTo("STUDENT"));
        Assert.That(authDTO?.Token, Is.Not.Null.And.Not.Empty);
      });
    }

  [Test]
  public async Task LoginBadCredentialTest()
  {
    // Arrange
    var email = "wrong.email@example.com";
    var password = "WrongPassword123!";

    _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync((ApplicationUser)null);

    // Act
    var result = await _controller!.Login(email, password) as ObjectResult;
    var response = result?.Value as Result;

    //Assert
    Assert.IsInstanceOf<Result>(response);
    Assert.That(response?.Code, Is.EqualTo(401));
    Assert.That(response?.Message, Is.EqualTo("username or password is incorrect"));
  }
  
  [Test]
  public async Task FindUserByIdSuccessTest()
{
    // Arrange
    var userId = 1;
    var user = new ApplicationUser
    {
        Id = userId,
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@gmail.com",
        PhoneNumber = "3333333333"
    };

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

    var userQualifiedPositions = new List<UserQualifiedPosition>
    {
        new()
        {
            UserId = userId,
            PositionId = 1,
            Position = new Position { PositionId = 1, PositionName = "DIRECTOR" }
        },
        new()
        {
            UserId = userId,
            PositionId = 2,
            Position = new Position { PositionId = 2, PositionName = "PRODUCER" }
        }
    };

    // Mock DbSets for Positions and UserQualifiedPositions
    var mockPositionSet = CreateMockDbSet(positions);
    var mockUserQualifiedPositionSet = CreateMockDbSet(userQualifiedPositions);

    // Setup mock context
    _mockContext?.Setup(c => c.Positions).Returns(mockPositionSet.Object);
    _mockContext?.Setup(c => c.UserQualifiedPositions).Returns(mockUserQualifiedPositionSet.Object);
    _mockContext?.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(user);
    _mockContext?.Setup(c => c.UserQualifiedPositions).ReturnsDbSet(userQualifiedPositions);

    // Act
    var result = await _controller!.FindUserById(userId) as ObjectResult;
    var response = result?.Value as Result;

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); // Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
        Assert.That(response?.Message, Is.EqualTo("Find Success")); // Verify Message
    });

    // Check that data is correctly returned as a UserDTO
    var foundUserDto = response?.Data as FoundUserDTO;
    Assert.That(foundUserDto, Is.Not.Null);
    Assert.That(foundUserDto?.UserId, Is.EqualTo(user.Id));

    // Verify UserQualifiedPositions query works as expected
    var retrievedPositions = foundUserDto?.Positions ?? new List<string>();
    var expectedPositions = positions.Select(p => p.PositionName).ToList();

    Assert.That(retrievedPositions, Is.EquivalentTo(expectedPositions));

    // Verify FindAsync was called once
    _mockContext?.Verify(c => c.Users.FindAsync(userId), Times.Once);
}


[Test]
public async Task FindUserByIdBadRequestTest()
{
    // Arrange
    int userId = 999; // Non-existent user ID
    _mockContext?.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync((ApplicationUser)null);

    // Act
    var result = await _controller!.FindUserById(userId) as ObjectResult;
    var response = result?.Value as Result;

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.False); // Verify Flag
        Assert.That(response?.Code, Is.EqualTo(404)); // Verify Code
        Assert.That(response?.Message, Is.EqualTo($"User with ID {userId} not found.")); // Verify Message
    });
}

  [Test]
    public async Task GetUsersTestSuccess()
    {
      // Arrange
      var users = new List<ApplicationUser>
        {
        new ApplicationUser { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "1234567890" },
        new ApplicationUser { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "0987654321" }
        };

      _mockContext?.Setup(c => c.Users).ReturnsDbSet(users);

      // Act
      var result = await _controller!.GetUsers() as ObjectResult;
      var response = result?.Value as Result;

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(result, Is.Not.Null);
        Assert.That(response?.Flag, Is.True); // Verify Flag
        Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
        Assert.That(response?.Message, Is.EqualTo("Found Users")); // Verify Message
      });

      var userDTOs = response?.Data as List<UserSimpleDTO>;
      Assert.That(userDTOs, Is.Not.Null);
      Assert.That(userDTOs?.Count, Is.EqualTo(users.Count));
      for (int i = 0; i < users.Count; i++)
      {
        Assert.Multiple(() =>
        {
          Assert.That(userDTOs?[i].UserId, Is.EqualTo(users[i].Id));
          Assert.That(userDTOs?[i].FullName, Is.EqualTo($"{users[i].FirstName} {users[i].LastName}"));
          Assert.That(userDTOs?[i].Email, Is.EqualTo(users[i].Email));
          Assert.That(userDTOs?[i].PhoneNumber, Is.EqualTo(users[i].PhoneNumber));
        });
      }
    }
  }

} 
