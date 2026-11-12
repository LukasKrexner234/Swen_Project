using System.Net;
using System.Text.Json;

namespace Project.Presentation;

public class Router
{
    private readonly AuthController _authController = new();

    public async Task<(int statusCode, string json)> RouteAsync(HttpListenerRequest request)
    {
        string method = request.HttpMethod;

        string path =
            request.Url?.AbsolutePath.TrimEnd('/') ?? "";

        if (path == "")
        {
            path = "/";
        }

        // GET /
        if (method == "GET" && path == "/")
        {
            return (
                200,
                JsonSerializer.Serialize(new
                {
                    message = "Welcome to MRP API"
                })
            );
        }

        // GET /api/media
        if (method == "GET" && path == "/api/media")
        {
            return (
                200,
                JsonSerializer.Serialize(new
                {
                    media = Array.Empty<object>()
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

        // Unknown endpoint
        return (
            404,
            """{"error":"Endpoint not found"}"""
        );
    }
}