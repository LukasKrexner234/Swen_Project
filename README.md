# Swen_Project
Repository for C# Swen Project

Protocol

Project Structure:

Presentation Layer:
Diese Schicht verarbeitet eingehende HTTP-Anfragen
- HttpServer.cs: startet den HTTP-Server und wartet auf eingehende Anfragen
- Router.cs: Leitet HTTP-Anfragen an den passenden Controller weiter
- AuthController.cs: verarbeitet Registrierung und Anmeldung von User
- MediaController.cs: verarbeitet CRUD-Anfragen und überprüft Zugriffsberechtigungen

Business Layer:
Diese Schicht enthält die Geschäftslogik
- AuthService.cs: verwaltet Registrierung, Passwort-Hashing, Anmeldung und Token-Erstellung
- MediaService.cs: überprüft Gültigkeit und koordiniert CRUD-Operationen

Data Access Layer:
Diese Schicht ist verantwortlich für die Kommunikation mit der PostgreSQL-Datenbank
- DatabaseConnection.cs: stellt Verbindung zur Datenbank her
- UserRepository.cs: verwaltet Benutzerdaten und Tokens
- MediaRepository.cs: führt SQL-Abfragen für CRUD aus

Domain Layer:
Diese Schicht enthält die Datenmodelle:
- User.cs: für User
- MediaEntry.cs: für Medienbeitrag
- Rating.cs: für Bewertungen

Technical Decisions:

HTTP-Server:
Für die Verarbeitung von HTTP-Anfragen wird ein HTTP-Listener verwendet.
Die Weiterleitung erfolgt über einen Router.

PostgeSQL:
PostgreSQL wird als Datenbank verwendet, um User, Tokens und Medieneinträge dauerhaft zu speichern.

Docker:
Die Postgres Datenbank wird von einem Docker ausgeführt.
Durch Verwendung von einem Docker Volume bleiben die Daten auch nach einem Neustart oder Stoppen erhalten.

Layered Architecture:
Presentation: verarbeitet HTTP-Anfragen
Business: Geschäftslogik und Validierung
DataAccess: Kommunikation mit der Datenbank
Domain: Datenmodelle

Asynchrone Programmierung:
Durch die Verwendung von async können Ein- und Ausgabeoperationen durchgeführt werden, ohne den aufgerufenen Thread zu blockieren.

Link zum Repo:
https://github.com/LukasKrexner234/Swen_Project.git
