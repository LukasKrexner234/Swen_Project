using System.Security.Cryptography;
using Project.DataAccess;
using Project.Domain;

namespace Project.Business;

public class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService()
    {
        _userRepository = new UserRepository();
    }

    // Registrierung
    public async Task<bool> RegisterAsync(
        string username,
        string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (username.Length > 100 || password.Length < 8)
        {
            return false;
        }

        User? existingUser =
            await _userRepository.GetUserByUsernameAsync(username);

        if (existingUser != null)
        {
            return false;
        }

        // Passwort sicher hashen
        string passwordHash = HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash
        };

        await _userRepository.CreateUserAsync(user);

        return true;
    }

    // Login
    public async Task<string?> LoginAsync(
        string username,
        string password)
    {
        User? user =
            await _userRepository.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return null;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        // Zufälligen Token erstellen
        string token =
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        // Token in PostgreSQL speichern
        await _userRepository.SaveTokenAsync(user.Id, token);

        return token;
    }

    // Passwort hashen
    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32
        );

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    // Passwort überprüfen
    private static bool VerifyPassword(
        string password,
        string storedHash)
    {
        string[] parts = storedHash.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32
        );

        return CryptographicOperations.FixedTimeEquals(
            actualHash,
            expectedHash
        );
    }
}