using Npgsql;
using Project.Domain;

namespace Project.DataAccess;

public class MediaRepository
{
    // CREATE
    public async Task<int> CreateMediaAsync(MediaEntry media)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            INSERT INTO media_entries
                (title, description, media_type, release_year,
                 genre, age_restriction, created_by)
            VALUES
                (@title, @description, @media_type, @release_year,
                 @genre, @age_restriction, @created_by)
            RETURNING id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        AddMediaParameters(command, media);
        command.Parameters.AddWithValue("created_by", media.CreatedBy);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    // READ: Alle Medien
    public async Task<List<MediaEntry>> GetAllMediaAsync()
    {
        var mediaList = new List<MediaEntry>();

        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            SELECT id, title, description, media_type,
                   release_year, genre, age_restriction, created_by
            FROM media_entries
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            mediaList.Add(MapMedia(reader));
        }

        return mediaList;
    }

    // READ: Einzelnes Medium
    public async Task<MediaEntry?> GetMediaByIdAsync(int id)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            SELECT id, title, description, media_type,
                   release_year, genre, age_restriction, created_by
            FROM media_entries
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapMedia(reader);
        }

        return null;
    }

    // UPDATE
    public async Task<bool> UpdateMediaAsync(MediaEntry media)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            UPDATE media_entries
            SET title = @title,
                description = @description,
                media_type = @media_type,
                release_year = @release_year,
                genre = @genre,
                age_restriction = @age_restriction
            WHERE id = @id
              AND created_by = @created_by;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        AddMediaParameters(command, media);

        command.Parameters.AddWithValue("id", media.Id);
        command.Parameters.AddWithValue("created_by", media.CreatedBy);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    // DELETE
    public async Task<bool> DeleteMediaAsync(int id, int userId)
    {
        await using var connection =
            await DatabaseConnection.GetConnectionAsync();

        const string sql = """
            DELETE FROM media_entries
            WHERE id = @id
              AND created_by = @user_id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("user_id", userId);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    // Hilfsmethode: SQL-Parameter setzen
    private static void AddMediaParameters(
        NpgsqlCommand command,
        MediaEntry media)
    {
        command.Parameters.AddWithValue("title", media.Title);
        command.Parameters.AddWithValue("description", media.Description);
        command.Parameters.AddWithValue("media_type", media.MediaType);
        command.Parameters.AddWithValue("release_year", media.ReleaseYear);
        command.Parameters.AddWithValue("genre", media.Genre);
        command.Parameters.AddWithValue("age_restriction", media.AgeRestriction);
    }

    // Hilfsmethode: Datenbankzeile in MediaEntry umwandeln
    private static MediaEntry MapMedia(NpgsqlDataReader reader)
    {
        return new MediaEntry
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
            MediaType = reader.GetString(3),
            ReleaseYear = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
            Genre = reader.IsDBNull(5) ? "" : reader.GetString(5),
            AgeRestriction = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
            CreatedBy = reader.GetInt32(7)
        };
    }
}