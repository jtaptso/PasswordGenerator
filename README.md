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
- **BCrypt.Net-Next** — password hashing with built-in salt
- **Swagger / OpenAPI** — interactive API documentation

## Features

- **Password Generation** — configurable length, character pools (uppercase, lowercase, digits, special), ambiguous character exclusion
- **Passphrase Generation** — word-based passphrases with configurable word count and separator
- **Strength Indicator** — entropy-based scoring with visual colored bar
- **Password Vault** — save, list, reveal, copy, edit, and delete password entries
- **Per-User Vault** — each user has their own isolated password vault
- **User Registration** — register with username, email, and password (BCrypt hashed)
- **Role-Based Authorization** — Admin and User roles, many-to-many (a user can have multiple roles)
- **Clipboard Copy** — one-click copy via JS interop
- **JWT Authentication** — multi-user auth with role claims
- **Swagger UI** — test all API endpoints with Bearer token support
- **Admin Dashboard** — manage users: list, promote to Admin, demote, delete with inline confirmation

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

### Default Admin Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `P@ssw0rd!` |

A default admin account is seeded at startup with the **Admin** and **User** roles. New users can register via `POST /api/auth/register` and receive the **User** role by default.

> **Note:** For production, configure the seeded admin credentials and JWT key via [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables.

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
| `POST` | `/api/auth/register` | No | Register a new user |
| `POST` | `/api/auth/login` | No | Get JWT token |
| `POST` | `/api/generator/password` | Yes | Generate a password |
| `POST` | `/api/generator/passphrase` | Yes | Generate a passphrase |
| `POST` | `/api/generator/strength` | Yes | Calculate password strength |
| `GET` | `/api/vault` | Yes | List own saved entries |
| `GET` | `/api/vault/{id}` | Yes | Get own entry (with decrypted password) |
| `POST` | `/api/vault` | Yes | Save a new entry |
| `PUT` | `/api/vault/{id}` | Yes | Update own entry |
| `DELETE` | `/api/vault/{id}` | Yes | Delete own entry |
| `POST` | `/api/admin/users/{userId}/roles` | Admin | Assign role to user |
| `DELETE` | `/api/admin/users/{userId}/roles/{roleName}` | Admin | Remove role from user |
| `GET` | `/api/admin/users` | Admin | List all users |
| `DELETE` | `/api/admin/users/{userId}` | Admin | Delete user |

## Configuration

### API (`src/PasswordGenerator.API/appsettings.json`)

| Key | Description |
|---|---|
| `Jwt:Key` | Symmetric signing key (min 32 chars) |
| `Jwt:Issuer` | Token issuer |
| `Jwt:Audience` | Token audience |
| `Jwt:ExpiryMinutes` | Token lifetime |
| `Seed:AdminUsername` | Seeded admin username (default: `admin`) |
| `Seed:AdminPassword` | Seeded admin password |
| `Registration:Enabled` | Enable/disable public registration (default: `true`) |

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
- User passwords are hashed with BCrypt (built-in salt, configurable work factor)
- Password generation uses `System.Security.Cryptography.RandomNumberGenerator` (CSPRNG)
- JWT tokens are signed with HMAC-SHA256 and include role claims
- Per-user vault isolation — users can only access their own password entries
- The dev SSL certificate bypass in the Web project should be removed for production

## License

MIT
