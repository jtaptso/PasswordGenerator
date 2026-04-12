# Password Generator — Clean Architecture with Separate API + Blazor Server UI

## Summary

Standalone ASP.NET Core Web API (reusable, JWT-secured) + separate Blazor Server UI that consumes it via `HttpClient`. Clean Architecture across 5 projects. Built with .NET 10.

## Architecture & Dependency Flow

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

- **Domain** — entities, enums (no dependencies)
- **Application** — interfaces, DTOs, pure services (depends on Domain)
- **Infrastructure** — EF Core, encryption, repository (implements Application interfaces)
- **API** — controllers, JWT config, composition root (references Application + Infrastructure)
- **Web** — Blazor Server UI, calls API via HttpClient (references Application for DTOs only)

## Tech Stack

| Layer          | Technology                                   |
|----------------|----------------------------------------------|
| UI             | Blazor Server (.NET 10, SignalR)             |
| API            | ASP.NET Core Web API (.NET 10)               |
| Database       | SQL Server + Entity Framework Core           |
| Encryption     | ASP.NET Core Data Protection API             |
| Authentication | JWT Bearer Tokens                            |
| Communication  | REST (HttpClient)                            |

## Project Structure

```
PasswordGenerator/
├── PasswordGenerator.sln
│
├── src/
│   ├── PasswordGenerator.Domain/
│   │   ├── Entities/
│   │   │   └── PasswordEntry.cs
│   │   └── Enums/
│   │       └── PasswordStrength.cs
│   │
│   ├── PasswordGenerator.Application/
│   │   ├── Interfaces/
│   │   │   ├── IPasswordGeneratorService.cs
│   │   │   ├── IEncryptionService.cs
│   │   │   └── IVaultRepository.cs
│   │   ├── DTOs/
│   │   │   ├── GenerateRequest.cs
│   │   │   ├── GenerateResult.cs
│   │   │   ├── PasswordEntryDto.cs
│   │   │   ├── LoginRequest.cs
│   │   │   └── TokenResponse.cs
│   │   └── Services/
│   │       ├── PasswordGeneratorService.cs
│   │       ├── PasswordStrengthCalculator.cs
│   │       └── VaultService.cs
│   │
│   ├── PasswordGenerator.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   └── PasswordEntryConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   └── VaultRepository.cs
│   │   ├── Services/
│   │   │   └── EncryptionService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── PasswordGenerator.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── GeneratorController.cs
│   │   │   └── VaultController.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── PasswordGenerator.Web/
│       ├── Components/
│       │   ├── Pages/
│       │   │   ├── Generator.razor
│       │   │   ├── Vault.razor
│       │   │   └── Login.razor
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   └── NavMenu.razor
│       │   └── PasswordStrengthIndicator.razor
│       ├── Services/
│       │   ├── PasswordApiClient.cs
│       │   └── TokenService.cs
│       ├── Program.cs
│       └── appsettings.json
```

## Database Schema

### Table: `PasswordEntries`

| Column            | Type           | Notes                            |
|-------------------|----------------|----------------------------------|
| Id                | int (PK)       | Auto-increment                   |
| Label             | nvarchar(200)  | User-given name (e.g. "Gmail")   |
| EncryptedPassword | nvarchar(max)  | Encrypted via Data Protection API|
| Website           | nvarchar(500)  | Optional URL                     |
| CreatedAt         | datetime2      | UTC                              |
| UpdatedAt         | datetime2      | UTC                              |

JWT auth uses a pre-configured username/password from user secrets — no `Users` table needed (single-user app).

## API Endpoints

| Method | Endpoint                    | Auth | Purpose              |
|--------|-----------------------------|------|----------------------|
| POST   | `/api/auth/login`           | No   | Get JWT token        |
| POST   | `/api/generator/password`   | Yes  | Generate password    |
| POST   | `/api/generator/passphrase` | Yes  | Generate passphrase  |
| POST   | `/api/generator/strength`   | Yes  | Calculate strength   |
| GET    | `/api/vault`                | Yes  | List saved entries   |
| GET    | `/api/vault/{id}`           | Yes  | Get single entry     |
| POST   | `/api/vault`                | Yes  | Save new entry       |
| PUT    | `/api/vault/{id}`           | Yes  | Update entry         |
| DELETE | `/api/vault/{id}`           | Yes  | Delete entry         |

## Implementation Phases

### Phase 1 — Solution Scaffolding

1. Create solution with 5 projects (3 class libraries + 2 web apps), set project references per dependency diagram
2. NuGet packages:
   - **Infrastructure**: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.AspNetCore.DataProtection`
   - **API**: `Microsoft.AspNetCore.Authentication.JwtBearer`
   - **Web**: default Blazor Server packages
3. Configure SQL Server connection string in API's `appsettings.json`
4. Configure API base URL in Web's `appsettings.json`

### Phase 2 — Domain & Application Layer

5. Define `PasswordEntry` entity and `PasswordStrength` enum in Domain
6. Define interfaces: `IPasswordGeneratorService`, `IEncryptionService`, `IVaultRepository`
7. Define DTOs: `GenerateRequest`, `GenerateResult`, `PasswordEntryDto`, `LoginRequest`, `TokenResponse`
8. Implement `PasswordGeneratorService` using `RandomNumberGenerator` — length, character pools, ambiguous exclusion, passphrase mode (embedded word list)
9. Implement `PasswordStrengthCalculator` — entropy-based scoring
10. Implement `VaultService` — orchestrates `IVaultRepository` + `IEncryptionService`

### Phase 3 — Infrastructure Layer

11. `AppDbContext` + `PasswordEntryConfiguration` (Fluent API)
12. `VaultRepository` implementing `IVaultRepository`
13. `EncryptionService` implementing `IEncryptionService` (wraps `IDataProtector`)
14. `DependencyInjection.cs` — `services.AddInfrastructure(configuration)` extension
15. Initial EF migration

### Phase 4 — Web API

16. JWT setup in `Program.cs`: `AddAuthentication().AddJwtBearer()` with key/issuer/audience from user secrets
17. `AuthController`: `POST /api/auth/login` — validates credentials, returns JWT
18. `GeneratorController` (`[Authorize]`): password, passphrase, strength endpoints
19. `VaultController` (`[Authorize]`): full CRUD for saved entries
20. Enable Swagger/OpenAPI

### Phase 5 — Blazor Server UI

21. `TokenService`: stores JWT in memory / protected browser storage
22. `PasswordApiClient`: typed `HttpClient` with JWT `Authorization` header
23. Login page (`/login`): username + password form
24. Generator page (`/`): length slider, character-type checkboxes, ambiguous toggle, passphrase toggle, Generate button, copy-to-clipboard (JS interop), strength indicator
25. `PasswordStrengthIndicator` component: colored bar
26. Vault page (`/vault`): table with reveal / copy / edit / delete actions
27. NavMenu: Login / Generator / Vault links (show/hide based on auth state)

## Verification Checklist

1. `dotnet build` — all 5 projects compile
2. Verify dependency flow: Domain → 0 refs, Application → Domain, Infrastructure → Application + Domain, API → Application + Infrastructure, Web → Application only
3. `dotnet ef database update` on API project — database created
4. Swagger: `POST /api/auth/login` returns JWT
5. Swagger: all generator and vault endpoints work with Bearer token
6. Confirm `EncryptedPassword` in SQL is not plaintext
7. Source uses `RandomNumberGenerator`, not `System.Random`
8. Blazor UI: login → generate → save → vault shows entry → reveal/edit/delete work
9. Passphrase mode + strength indicator work
10. API is callable independently (Postman/curl)

## Decisions

- **Separate API + UI** — API is independently deployable and reusable
- **JWT auth** — single pre-configured user (credentials in user secrets), no Users table
- **Clean Architecture** — 5 projects with strict dependency flow
- **Web references Application for DTOs only** — all data flows through the API
- **Encryption**: ASP.NET Core Data Protection API (machine-scoped keys by default)
- **Password generation**: `System.Security.Cryptography.RandomNumberGenerator` (cryptographically secure)

## Scope

**In scope**: generator, vault CRUD, encryption at rest, strength indicator, passphrase mode, JWT auth, Swagger

**Out of scope**: user registration, role-based auth, browser extension, import/export, categories/tags
