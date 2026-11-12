using Npgsql;
using Project.Domain;

namespace Project.DataAccess;

public class UserRepository
{
    // Benutzer erstellen
    public async Task<int> CreateUserAsync(User user)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            INSERT INTO users (username, password_hash)
            VALUES (@username, @password_hash)
            RETURNING id;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("username", user.Username);
        command.Parameters.AddWithValue("password_hash", user.PasswordHash);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    // Benutzer anhand des Benutzernamens suchen
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            SELECT id, username, password_hash, token
            FROM users
            WHERE username = @username;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("username", username);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Token = reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3)
            };
        }

        return null;
    }

    // Token speichern
    public async Task SaveTokenAsync(int userId, string token)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            UPDATE users
            SET token = @token
            WHERE id = @id;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("id", userId);

        await command.ExecuteNonQueryAsync();
    }

    // Benutzer anhand des Tokens suchen
    public async Task<User?> GetUserByTokenAsync(string token)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            SELECT id, username, password_hash, token
            FROM users
            WHERE token = @token;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("token", token);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Token = reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3)
            };
        }

        return null;
    }
}