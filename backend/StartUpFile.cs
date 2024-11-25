
using backend.Models;
using backend.Auth;

namespace backend;
public class StartUpFile() {

  public static void RunStartupFile() {

        using var _context = new FrogcrewContext();
        var userId = 1;
        var passwords = new List<string>();
        passwords = ["swiftie4lyfe", "awsom3sauce", "1<3Coffee", "superfrog", "h0ck3y4lif3", "password", "wordpass", "admin"];
        foreach (var password in passwords)
        {
            var user = _context.Users.Find(userId);
            user!.Password = PasswordHasher.HashPassword(password);
            userId++;
        }
        _context.SaveChanges();
  }
}