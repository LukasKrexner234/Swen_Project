using System.Net;
using System.Text.Json;
using Project.Business;
using Project.DataAccess;
using Project.Domain;

namespace Project.Presentation;

public class MediaController
{
    private readonly MediaService _mediaService = new();
    private readonly UserRepository _userRepository = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // GET /api/media
    public async Task<(int statusCode, string json)> GetAllAsync()
    {
        List<MediaEntry> media = await _mediaService.GetAllMediaAsync();

        return (200, JsonSerializer.Serialize(media, JsonOptions));
    }

    // GET /api/media/{id}
    public async Task<(int statusCode, string json)> GetByIdAsync(int id)
    {
        MediaEntry? media = await _mediaService.GetMediaByIdAsync(id);

        if (media == null)
        {
            return (404, """{"error":"Media not found"}""");
        }

        return (200, JsonSerializer.Serialize(media, JsonOptions));
    }

    // POST /api/media
    public async Task<(int statusCode, string json)> CreateAsync(HttpListenerRequest request)
    {
        User? user = await AuthenticateAsync(request);

        if (user == null)
        {
            return (401, """{"error":"Unauthorized"}""");
        }

        try
        {
            MediaEntry? media = await ReadMediaAsync(request);

            if (media == null)
            {
                return (400, """{"error":"Invalid media data"}""");
            }

            // Benutzer-ID kommt aus dem Token, nicht aus dem JSON
            media.CreatedBy = user.Id;

            int id = await _mediaService.CreateMediaAsync(media);

            return (201, JsonSerializer.Serialize(new { message = "Media created successfully", id }));
        }
        catch (JsonException)
        {
            return (400, """{"error":"Invalid JSON"}""");
        }
        catch (ArgumentException ex)
        {
            return (400, JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }

    // PUT /api/media/{id}
    public async Task<(int statusCode, string json)> UpdateAsync(HttpListenerRequest request, int id)
    {
        User? user = await AuthenticateAsync(request);

        if (user == null)
        {
            return (401, """{"error":"Unauthorized"}""");
        }

        try
        {
            MediaEntry? existing = await _mediaService.GetMediaByIdAsync(id);

            if (existing == null)
            {
                return (404, """{"error":"Media not found"}""");
            }

            if (existing.CreatedBy != user.Id)
            {
                return (403, """{"error":"Forbidden"}""");
            }

            MediaEntry? media = await ReadMediaAsync(request);

            if (media == null)
            {
                return (400, """{"error":"Invalid media data"}""");
            }

            media.Id = id;
            media.CreatedBy = user.Id;

            bool updated = await _mediaService.UpdateMediaAsync(media);

            if (!updated)
            {
                return (404, """{"error":"Media not found"}""");
            }

            return (200, """{"message":"Media updated successfully"}""");
        }
        catch (JsonException)
        {
            return (400, """{"error":"Invalid JSON"}""");
        }
        catch (ArgumentException ex)
        {
            return (400, JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }

    // DELETE /api/media/{id}
    public async Task<(int statusCode, string json)> DeleteAsync(HttpListenerRequest request, int id)
    {
        User? user = await AuthenticateAsync(request);

        if (user == null)
        {
            return (401, """{"error":"Unauthorized"}""");
        }

        MediaEntry? existing = await _mediaService.GetMediaByIdAsync(id);

        if (existing == null)
        {
            return (404, """{"error":"Media not found"}""");
        }

        if (existing.CreatedBy != user.Id)
        {
            return (403, """{"error":"Forbidden"}""");
        }

        bool deleted = await _mediaService.DeleteMediaAsync(id, user.Id);

        if (!deleted)
        {
            return (404, """{"error":"Media not found"}""");
        }

        return (204, "");
    }

    // Token überprüfen und Benutzer ermitteln
    private async Task<User?> AuthenticateAsync(HttpListenerRequest request)
    {
        string? authorization = request.Headers["Authorization"];

        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string token = authorization["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return await _userRepository.GetUserByTokenAsync(token);
    }

    // JSON-Body auslesen
    private static async Task<MediaEntry?> ReadMediaAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream);

        string body = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<MediaEntry>(body, JsonOptions);
    }
}