using System.Security.Cryptography;
using System.Text;

namespace OopsReviewCenter.Services;

public class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000;

    /// <summary>
    /// Generates a random salt for password hashing
    /// </summary>
    public string GenerateSalt()
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    /// <summary>
    /// Hashes a password with the given salt using PBKDF2
    /// </summary>
    public string HashPassword(string password, string salt)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        if (string.IsNullOrEmpty(salt))
            throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

        byte[] saltBytes = Convert.FromBase64String(salt);
        
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hashBytes = pbkdf2.GetBytes(HashSize);
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// Verifies a password against a hash and salt
    /// </summary>
    public bool VerifyPassword(string password, string salt, string hash)
    {
        string newHash = HashPassword(password, salt);
        return newHash == hash;
    }
}
