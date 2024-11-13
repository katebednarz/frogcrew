
using backend.Models;


public class StartUpFile() {

  public static void RunStartupFile() {
    using var _context = new FrogcrewContext();

    var passwords = new List<string>(); 
    passwords = ["swiftie4lyfe","awsom3sauce","1<3Coffee","superfrog","h0ck3y4lif3","password","wordpass"];

    var user =  _context.Users.Find(1);
    user.Password = PasswordHasher.HashPassword("swiftie4lyfe");
    _context.SaveChanges();

  }
}