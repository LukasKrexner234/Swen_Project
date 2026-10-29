using System.Net;
using System.Text;

namespace Project.Presentation;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly Router _router;

    public HttpServer()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8080/");

        _router = new Router();
    }

    public async Task StartAsync()
    {
        _listener.Start();

        Console.WriteLine("MRP Server running on http://localhost:8080");

        while (_listener.IsListening)
        {
            HttpListenerContext context =
                await _listener.GetContextAsync();

            await HandleRequestAsync(context);
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        Console.WriteLine(
            $"{request.HttpMethod} {request.Url?.AbsolutePath}"
        );

        try
        {
            (int statusCode, string json) = _router.Route(request);

            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);
        }
        catch (HttpListenerException ex)
        {
            Console.Error.WriteLine($"HTTP error: {ex.Message}");
        }
        catch (ObjectDisposedException ex)
        {
            Console.Error.WriteLine($"Response already closed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server error: {ex.Message}");

            try
            {
                if (response.OutputStream.CanWrite)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(
                        """{"error":"Internal server error"}"""
                    );

                    response.StatusCode = 500;
                    response.ContentType = "application/json; charset=utf-8";
                    response.ContentLength64 = buffer.Length;

                    await response.OutputStream.WriteAsync(buffer);
                }
            }
            catch (Exception responseException)
                when (responseException is HttpListenerException
                          or ObjectDisposedException
                          or InvalidOperationException)
            {
                Console.Error.WriteLine(
                    $"Could not send error response: {responseException.Message}"
                );
            }
        }
        finally
        {
            try
            {
                response.Close();
            }
            catch (ObjectDisposedException)
            {
                // Response was already closed.
            }
        }
    }
}