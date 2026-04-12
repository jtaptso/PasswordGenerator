# 🔐 Password Generator

A full-stack password generator and vault application built with **Clean Architecture** — featuring a standalone ASP.NET Core Web API secured with JWT and a Blazor Server UI that consumes it.

## Architecture

```
PasswordGenerator.Web            PasswordGenerator.API
(Blazor Server UI)               (ASP.NET Core Web API)
       │                                │
       ├──► HttpClient (REST/JWT) ──────┤
       │                                │
       └──► PasswordGenerator.Application ◄──┘
                     │
                     └──► PasswordGenerator.Domain

PasswordGenerator.Infrastructure
       │
       ├──► PasswordGenerator.Application
       └──► PasswordGenerator.Domain
```

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, enums (zero dependencies) |
| **Application** | Interfaces, DTOs, pure services (depends on Domain) |
| **Infrastructure** | EF Core, encryption, repository implementations |
| **API** | Controllers, JWT auth, composition root |
| **Web** | Blazor Server UI, API client |

## Tech Stack

- **.NET 10** — Blazor Server + ASP.NET Core Web API
- **Entity Framework Core** — InMemory provider (swappable to SQL Server)
- **ASP.NET Core Data Protection API** — password encryption at rest
- **JWT Bearer Tokens** — API authentication
- **Swagger / OpenAPI** — interactive API documentation

## Features

- **Password Generation** — configurable length, character pools (uppercase, lowercase, digits, special), ambiguous character exclusion
- **Passphrase Generation** — word-based passphrases with configurable word count and separator
- **Strength Indicator** — entropy-based scoring with visual colored bar
- **Password Vault** — save, list, reveal, copy, edit, and delete password entries
- **Clipboard Copy** — one-click copy via JS interop
- **JWT Authentication** — single-user auth with configurable credentials
- **Swagger UI** — test all API endpoints with Bearer token support

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run the API

```bash
cd src/PasswordGenerator.API
dotnet run --launch-profile https
```

The API starts at **https://localhost:7001**. Swagger UI is available at [https://localhost:7001/swagger](https://localhost:7001/swagger).

### Run the Web UI

In a separate terminal:

```bash
cd src/PasswordGenerator.Web
dotnet run --launch-profile https
```

The Blazor UI starts at **https://localhost:7236**.

### Default Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `P@ssw0rd!` |

> **Note:** For production, move credentials and JWT key to [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables.

## Project Structure

```
PasswordGenerator/
├── PasswordGenerator.sln
└── src/
    ├── PasswordGenerator.Domain/          # Entities, Enums
    ├── PasswordGenerator.Application/     # Interfaces, DTOs, Services
    ├── PasswordGenerator.Infrastructure/  # EF Core, Encryption, Repositories
    ├── PasswordGenerator.API/             # Web API, Controllers, JWT
    └── PasswordGenerator.Web/             # Blazor Server UI
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | No | Get JWT token |
| `POST` | `/api/generator/password` | Yes | Generate a password |
| `POST` | `/api/generator/passphrase` | Yes | Generate a passphrase |
| `POST` | `/api/generator/strength` | Yes | Calculate password strength |
| `GET` | `/api/vault` | Yes | List all saved entries |
| `GET` | `/api/vault/{id}` | Yes | Get entry (with decrypted password) |
| `POST` | `/api/vault` | Yes | Save a new entry |
| `PUT` | `/api/vault/{id}` | Yes | Update an entry |
| `DELETE` | `/api/vault/{id}` | Yes | Delete an entry |

## Configuration

### API (`src/PasswordGenerator.API/appsettings.json`)

| Key | Description |
|---|---|
| `Jwt:Key` | Symmetric signing key (min 32 chars) |
| `Jwt:Issuer` | Token issuer |
| `Jwt:Audience` | Token audience |
| `Jwt:ExpiryMinutes` | Token lifetime |
| `Auth:Username` | Login username |
| `Auth:Password` | Login password |

### Web (`src/PasswordGenerator.Web/appsettings.json`)

| Key | Description |
|---|---|
| `ApiBaseUrl` | Base URL of the API (default: `https://localhost:7001`) |

## Switching to SQL Server

Replace the InMemory provider in `src/PasswordGenerator.Infrastructure/DependencyInjection.cs`:

```csharp
// Change this:
options.UseInMemoryDatabase("PasswordGeneratorDb");

// To this:
options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
```

Then run migrations:

```bash
cd src/PasswordGenerator.API
dotnet ef migrations add InitialCreate --project ../PasswordGenerator.Infrastructure
dotnet ef database update
```

## Security Notes

- Passwords are encrypted at rest using the ASP.NET Core Data Protection API
- Password generation uses `System.Security.Cryptography.RandomNumberGenerator` (CSPRNG)
- JWT tokens are signed with HMAC-SHA256
- The dev SSL certificate bypass in the Web project should be removed for production

## License

MIT
