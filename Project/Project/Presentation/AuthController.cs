using System.Net;
using System.Text.Json;
using Npgsql;
using Project.Business;

namespace Project.Presentation;

public class AuthController
{
    private readonly AuthService _authService = new();

    public async Task<(int statusCode, string json)> RegisterAsync(HttpListenerRequest request)
    {
        try
        {
            using var reader = new StreamReader(request.InputStream);

            string body = await reader.ReadToEndAsync();

            using JsonDocument document = JsonDocument.Parse(body);

            string username =
                document.RootElement.GetProperty("username").GetString() ?? "";

            string password =
                document.RootElement.GetProperty("password").GetString() ?? "";

            bool success =
                await _authService.RegisterAsync(username, password);

            if (!success)
            {
                return (
                    400,
                    JsonSerializer.Serialize(new
                    {
                        error = "Invalid registration data or username already exists"
                    })
                );
            }

            return (
                201,
                JsonSerializer.Serialize(new
                {
                    message = "User registered successfully"
                })
            );
        }
        catch (PostgresException ex)
            when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return (
                409,
                """{"error":"Username already exists"}"""
            );
        }
        catch (JsonException)
        {
            return (
                400,
                """{"error":"Invalid JSON"}"""
            );
        }
        catch (KeyNotFoundException)
        {
            return (
                400,
                """{"error":"Missing username or password"}"""
            );
        }
        catch (InvalidOperationException)
        {
            return (
                400,
                """{"error":"Invalid username or password format"}"""
            );
        }
    }

    public async Task<(int statusCode, string json)> LoginAsync(HttpListenerRequest request)
    {
        try
        {
            using var reader = new StreamReader(request.InputStream);

            string body = await reader.ReadToEndAsync();

            using JsonDocument document = JsonDocument.Parse(body);

            string username =
                document.RootElement.GetProperty("username").GetString() ?? "";

            string password =
                document.RootElement.GetProperty("password").GetString() ?? "";

            string? token =
                await _authService.LoginAsync(username, password);

            if (token == null)
            {
                return (
                    401,
                    """{"error":"Invalid credentials"}"""
                );
            }

            return (
                200,
                JsonSerializer.Serialize(new
                {
                    token
                })
            );
        }
        catch (JsonException)
        {
            return (
                400,
                """{"error":"Invalid JSON"}"""
            );
        }
        catch (KeyNotFoundException)
        {
            return (
                400,
                """{"error":"Missing username or password"}"""
            );
        }
        catch (InvalidOperationException)
        {
            return (
                400,
                """{"error":"Invalid username or password format"}"""
            );
        }
    }
}