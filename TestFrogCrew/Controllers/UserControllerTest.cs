using Moq;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTO;
using backend.Controllers;
using backend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using misc;

namespace TestFrogCrew.Controllers
{
  [TestFixture()]
  public class UserControllerTest
  {
    private FrogcrewContext _context;
    private IConfiguration? _config;
    private UserController? _controller;
    private UserManager<ApplicationUser> _userManager;
    private SignInManager<ApplicationUser> _signInManager;
    private DatabaseHelper? _dbHelper;
    private NotificationsHelper _notificationHelper;

    [SetUp]
    public async Task Setup()
    {
      var options = new DbContextOptionsBuilder<FrogcrewContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;
      
      _context = new FrogcrewContext(options);
      
      _context.Database.EnsureDeleted();
      _context.Database.EnsureCreated();
      
      var userStore = new UserStore<ApplicationUser,ApplicationRole,FrogcrewContext,int>(_context);
      
      _userManager = new UserManager<ApplicationUser>(
        userStore,
        null, // No password validators needed for testing
        new PasswordHasher<ApplicationUser>(),
        new List<IUserValidator<ApplicationUser>>(),
        new List<IPasswordValidator<ApplicationUser>>(),
        null,
        null,
        null,
        null
      );
      
      _signInManager = new SignInManager<ApplicationUser>(
        _userManager,
        new Mock<IHttpContextAccessor>().Object,
        new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
        null,
        null,
        null,
        null
      );

      var user = new ApplicationUser
      {
        UserName = "john.doe@gmail.com",
        Email = "john.doe@gmail.com",
        PhoneNumber = "0123456789",
        FirstName = "John",
        LastName = "Doe",
        IsActive = true
      };
      
      // Adding Test Data
      _context.Roles.AddRange(
        new ApplicationRole{ Id = 1, Name = "STUDENT", NormalizedName = "STUDENT" },
        new ApplicationRole{ Id = 2, Name = "UPDATED_ROLE", NormalizedName = "UPDATED_ROLE"}
      );
      
      _context.Users.AddRange(
        new ApplicationUser { Id = 1, FirstName = "Kate", LastName = "Bednarz", Email = "kate.bednarz@tcu.edu", IsActive = true},
        new ApplicationUser { Id = 2, FirstName = "Aliya", LastName = "Suri", IsActive = true}
      );
      
      _context.Positions.AddRange(
        new Position {PositionId = 1, PositionName = "DIRECTOR", PositionLocation = "CONTROL ROOM"},
        new Position {PositionId = 2, PositionName = "PRODUCER", PositionLocation = "CONTROL ROOM"},
        new Position { PositionId = 3, PositionName = "UPDATED_POSITION_1", PositionLocation = "CONTROL ROOM" },
        new Position { PositionId = 4, PositionName = "UPDATED_POSITION_2", PositionLocation = "CONTROL ROOM"}
      );
      
      _context.UserQualifiedPositions.AddRange(
        new UserQualifiedPosition {PositionId = 1, UserId = 1},
        new UserQualifiedPosition {PositionId = 1, UserId = 3}
      );
      
      _context.Invitations.AddRange(
        new Invitation {Token = "token"}
      );
      
      _context.SaveChanges();
      
      await _userManager.CreateAsync(user, "Password!1");
      await _userManager.AddToRoleAsync(user, "STUDENT");
      
      _notificationHelper = new NotificationsHelper(_context);
      _config = new ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .Build();

      _dbHelper = new DatabaseHelper(_context);
      _controller = new UserController(
        _userManager, 
        _signInManager,
        _context,
        _config,
        _notificationHelper);
    }

    [TearDown]
    public void Teardown()
    {
      _context?.Dispose();
      _controller?.Dispose();
      _userManager?.Dispose();
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
      var request = new UserDTO
      {
        Email = "test@example.com",
        FirstName = "John",
        LastName = "Doe",
        PhoneNumber = "1234567890",
        Role = "STUDENT",
        Position = ["DIRECTOR", "PRODUCER"]
      };
      
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
      var userDto = response?.Data as UserDTO;
      Assert.That(userDto, Is.Not.Null);
      Assert.Multiple(() =>
      {
        Assert.That(userDto?.Email, Is.EqualTo(request.Email));
        Assert.That(userDto?.FirstName, Is.EqualTo(request.FirstName));
        Assert.That(userDto?.LastName, Is.EqualTo(request.LastName));
        Assert.That(userDto?.PhoneNumber, Is.EqualTo(request.PhoneNumber));
        Assert.That(userDto?.Role, Is.EqualTo(request.Role));
      });
      
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
         string token = "token";

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
       var email = "john.doe@gmail.com";
       var password = "Password!1";
       
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
         Assert.That(authDTO?.UserId, Is.EqualTo(3));
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
    var userId = 3;

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
    var foundUserDto = response?.Data as FoundUserDTO;
    Assert.That(foundUserDto, Is.Not.Null);
    Assert.Multiple(() =>
      {
        Assert.That(foundUserDto?.UserId, Is.EqualTo(userId));
        Assert.That(foundUserDto?.FirstName, Is.EqualTo("John"));
        Assert.That(foundUserDto?.LastName, Is.EqualTo("Doe"));
        Assert.That(foundUserDto?.Positions[0], Is.EqualTo("DIRECTOR"));
      }
    );
}

[Test]
public async Task FindUserByIdBadRequestTest()
{
    // Arrange
    int userId = 999; // Non-existent user ID

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
   public async Task UpdateUserByIdSuccessTest()
   {
       // Arrange
       var userId = 3;
       var request = new UserDTO
       {
           FirstName = "UpdatedFirstName",
           LastName = "UpdatedLastName",
           Email = "updated.email@example.com",
           PhoneNumber = "9876543210",
           Role = "UPDATED_ROLE",
           Position = new List<string> { "UPDATED_POSITION_1", "UPDATED_POSITION_2" }
       };

       // Act
       var result = await _controller!.UpdateUserByUserId(request, userId) as ObjectResult;
       var response = result?.Value as Result;

       // Assert
       Assert.Multiple(() =>
       {
           Assert.That(result, Is.Not.Null);
           Assert.That(response?.Flag, Is.True); // Verify Flag
           Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
           Assert.That(response?.Message, Is.EqualTo("Update Success")); // Verify Message
       });

       // Verify the updated user details
       var updatedUser = response?.Data as FoundUserDTO;
       Assert.That(updatedUser, Is.Not.Null);
       Assert.Multiple(() =>
       {
           Assert.That(updatedUser?.UserId, Is.EqualTo(userId));
           Assert.That(updatedUser?.FirstName, Is.EqualTo(request.FirstName));
           Assert.That(updatedUser?.LastName, Is.EqualTo(request.LastName));
           Assert.That(updatedUser?.Email, Is.EqualTo(request.Email));
           Assert.That(updatedUser?.PhoneNumber, Is.EqualTo(request.PhoneNumber));
           Assert.That(updatedUser?.Role, Is.EqualTo(request.Role));
           Assert.That(updatedUser?.Positions, Is.EquivalentTo(request.Position));
       });
   }

   [Test]
   public async Task UpdateUserByIdUserNotFoundTest()
   {
     // Arrange
     int userId = 999; // Non-existent user ID

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
   public async Task DisableUserByIdSuccessTest()
   {
     // Arrange
     var userId = 1;

     // Act
     var result = await _controller!.DisableUser(userId) as ObjectResult;
     var response = result?.Value as Result;

     // Assert
     Assert.Multiple(() =>
     {
       Assert.That(result, Is.Not.Null);
       Assert.That(response?.Flag, Is.True); // Verify Flag
       Assert.That(response?.Code, Is.EqualTo(200)); // Verify Code
       Assert.That(response?.Message, Is.EqualTo("Disable Success")); // Verify Message
     });
     
     var user = _context.Users.SingleOrDefault(u => u.Id == userId);
     Assert.That(user.IsActive, Is.False);
     
   }

   [Test]
   public async Task DisableUserByIdUserNotFoundTest()
   {
     // Arrange
     int userId = 999; // Non-existent user ID

     // Act
     var result = await _controller!.DisableUser(userId) as ObjectResult;
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
       Assert.That(userDTOs?.Count, Is.EqualTo(1));
     }
   }
} 
