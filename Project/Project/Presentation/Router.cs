using System.Net;
using System.Text.Json;

namespace Project.Presentation;

public class Router
{
    public (int statusCode, string json) Route(HttpListenerRequest request)
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
            return (
                501,
                JsonSerializer.Serialize(new
                {
                    error = "Registration not implemented yet"
                })
            );
        }

        // POST /api/users/login
        if (method == "POST" && path == "/api/users/login")
        {
            return (
                501,
                JsonSerializer.Serialize(new
                {
                    error = "Login not implemented yet"
                })
            );
        }

        // Unknown endpoint
        return (
            404,
            JsonSerializer.Serialize(new
            {
                error = "Endpoint not found"
            })
        );
    }
}