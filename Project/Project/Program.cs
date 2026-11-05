using Project.Presentation;
using Project.DataAccess;

// PostgreSQL-Verbindung testen
await DatabaseConnection.TestConnection();

// HTTP-Server starten
HttpServer server = new HttpServer();

await server.StartAsync();