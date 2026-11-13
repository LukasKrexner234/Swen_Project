using System.Net;
using System.Text.Json;

namespace Project.Presentation;

public class Router
{
    private readonly AuthController _authController = new();
    private readonly MediaController _mediaController = new();

    public async Task<(int statusCode, string json)> RouteAsync(HttpListenerRequest request)
    {
        string method = request.HttpMethod;

        string path = request.Url?.AbsolutePath.TrimEnd('/') ?? "";

        if (path == "")
        {
            path = "/";
        }

        // GET /
        if (method == "GET" && path == "/")
        {
            return (200, JsonSerializer.Serialize(new
                {
                    message = "Welcome to MRP API"
                })
            );
        }

        // POST /api/users/register
        if (method == "POST" && path == "/api/users/register")
        {
            return await _authController.RegisterAsync(request);
        }

        // POST /api/users/login
        if (method == "POST" && path == "/api/users/login")
        {
            return await _authController.LoginAsync(request);
        }

        // GET /api/media
        if (method == "GET" && path == "/api/media")
        {
            return await _mediaController.GetAllAsync();
        }

        // POST /api/media
        if (method == "POST" && path == "/api/media")
        {
            return await _mediaController.CreateAsync(request);
        }

        // Endpunkte mit Medien-ID
        if (path.StartsWith("/api/media/"))
        {
            string idString = path["/api/media/".Length..];

            if (!int.TryParse(idString, out int id) || id <= 0)
            {
                return (400, """{"error":"Invalid media ID"}""");
            }

            if (method == "GET")
            {
                return await _mediaController.GetByIdAsync(id);
            }

            if (method == "PUT")
            {
                return await _mediaController.UpdateAsync(request, id);
            }

            if (method == "DELETE")
            {
                return await _mediaController.DeleteAsync(request, id);
            }
        }

        // Unbekannter Endpunkt
        return (404, """{"error":"Endpoint not found"}""");
    }
}