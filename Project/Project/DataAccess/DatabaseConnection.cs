using Npgsql;

namespace Project.DataAccess;

public static class DatabaseConnection
{
    private static readonly string ConnectionString =
        "Host=127.0.0.1;" +
        "Port=5435;" +
        "Database=mrp;" +
        "Username=postgres;" +
        "Password=mysecretpassword;";

    public static async Task TestConnection()
    {
        await using var connection =
            new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        Console.WriteLine("PostgreSQL connection successful!");
    }
}