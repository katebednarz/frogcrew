using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace backend.Auth
{
    public class PasswordHasher
    {
        /*
            * Hashes the password using a random salt and PBKDF2
            * 
            * @param password The password to hash
            * @return The hashed password
        */
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
              password: password,
              salt: salt,
              prf: KeyDerivationPrf.HMACSHA256,
              iterationCount: 10000,
              numBytesRequested: 256 / 8
            ));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        /*
            * Verifies the password against the stored hash
            * 
            * @param enteredPassword The password entered by the user
            * @param storedHash The stored hash
            * @return True if the password is correct, false otherwise
        */
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
              password: enteredPassword,
              salt: salt,
              prf: KeyDerivationPrf.HMACSHA256,
              iterationCount: 10000,
              numBytesRequested: 256 / 8
            ));

            return hashed == parts[1];
        }
    }
}
