# Password Generator — Clean Architecture with Separate API + Blazor Server UI

## Summary

Standalone ASP.NET Core Web API (reusable, JWT-secured) + separate Blazor Server UI that consumes it via `HttpClient`. Clean Architecture across 5 projects. Built with .NET 10.

Multi-user application with custom User/Role entities (no ASP.NET Identity). Users register with username, email, and password (hashed with BCrypt). Each user can have multiple roles (many-to-many). Two seeded roles: **Admin** and **User**. New registrations receive the "User" role by default; only Admins can assign additional roles. Each user has their own isolated password vault.

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
| Authorization  | Role-based (Admin, User) with custom entities|
| Password Hash  | BCrypt (via BCrypt.Net-Next)                 |
| Communication  | REST (HttpClient)                            |

## Project Structure

```
PasswordGenerator/
├── PasswordGenerator.sln
│
├── src/
│   ├── PasswordGenerator.Domain/
│   │   ├── Entities/
│   │   │   ├── PasswordEntry.cs
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   └── UserRole.cs
│   │   └── Enums/
│   │       └── PasswordStrength.cs
│   │
│   ├── PasswordGenerator.Application/
│   │   ├── Interfaces/
│   │   │   ├── IPasswordGeneratorService.cs
│   │   │   ├── IEncryptionService.cs
│   │   │   ├── IVaultRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   └── IPasswordHasher.cs
│   │   ├── DTOs/
│   │   │   ├── GenerateRequest.cs
│   │   │   ├── GenerateResult.cs
│   │   │   ├── PasswordEntryDto.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── TokenResponse.cs
│   │   │   ├── RegisterRequest.cs
│   │   │   └── RegisterResult.cs
│   │   └── Services/
│   │       ├── PasswordGeneratorService.cs
│   │       ├── PasswordStrengthCalculator.cs
│   │       ├── VaultService.cs
│   │       └── AuthService.cs
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
│   │   │   ├── VaultController.cs
│   │   │   └── AdminController.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── PasswordGenerator.Web/
│       ├── Components/
│       │   ├── Pages/
│       │   │   ├── Generator.razor
│       │   │   ├── Vault.razor
│       │   │   ├── Login.razor
│       │   │   └── Register.razor
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

### Table: `Users`

| Column       | Type           | Notes                          |
|--------------|----------------|--------------------------------|
| Id           | int (PK)       | Auto-increment                 |
| Username     | nvarchar(50)   | Required, unique index         |
| Email        | nvarchar(200)  | Required, unique index         |
| PasswordHash | nvarchar(max)  | BCrypt hashed                  |
| CreatedAt    | datetime2      | UTC                            |
| UpdatedAt    | datetime2      | UTC                            |

### Table: `Roles`

| Column | Type          | Notes                              |
|--------|---------------|------------------------------------|
| Id     | int (PK)      | Auto-increment                     |
| Name   | nvarchar(50)  | Required, unique index             |

Seeded data: **Admin** (Id=1), **User** (Id=2).

### Table: `UserRoles`

| Column | Type     | Notes                        |
|--------|----------|------------------------------|
| UserId | int (FK) | Composite PK, FK to Users    |
| RoleId | int (FK) | Composite PK, FK to Roles    |

Many-to-many join table. A user can have multiple roles.

### Table: `PasswordEntries`

| Column            | Type           | Notes                            |
|-------------------|----------------|----------------------------------|
| Id                | int (PK)       | Auto-increment                   |
| UserId            | int (FK)       | FK to Users, cascade delete      |
| Label             | nvarchar(200)  | User-given name (e.g. "Gmail")   |
| EncryptedPassword | nvarchar(max)  | Encrypted via Data Protection API|
| Website           | nvarchar(500)  | Optional URL                     |
| CreatedAt         | datetime2      | UTC                              |
| UpdatedAt         | datetime2      | UTC                              |

Each user has their own isolated vault. Deleting a user cascades to their password entries.

## API Endpoints

| Method | Endpoint                           | Auth  | Purpose                    |
|--------|------------------------------------|-------|----------------------------|
| POST   | `/api/auth/register`               | No    | Register new user          |
| POST   | `/api/auth/login`                  | No    | Get JWT token              |
| POST   | `/api/generator/password`          | Yes   | Generate password          |
| POST   | `/api/generator/passphrase`        | Yes   | Generate passphrase        |
| POST   | `/api/generator/strength`          | Yes   | Calculate strength         |
| GET    | `/api/vault`                       | Yes   | List saved entries (own)   |
| GET    | `/api/vault/{id}`                  | Yes   | Get single entry (own)     |
| POST   | `/api/vault`                       | Yes   | Save new entry             |
| PUT    | `/api/vault/{id}`                  | Yes   | Update entry (own)         |
| DELETE | `/api/vault/{id}`                  | Yes   | Delete entry (own)         |
| POST   | `/api/admin/users/{userId}/roles`  | Admin | Assign role to user        |
| DELETE | `/api/admin/users/{userId}/roles/{roleName}` | Admin | Remove role from user |
| GET    | `/api/admin/users`                 | Admin | List all users             |
| DELETE | `/api/admin/users/{userId}`        | Admin | Delete user                |

## Implementation Phases

### Phase 1 — Solution Scaffolding

1. Create solution with 5 projects (3 class libraries + 2 web apps), set project references per dependency diagram
2. NuGet packages:
   - **Infrastructure**: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.AspNetCore.DataProtection`, `BCrypt.Net-Next`
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

### Phase 6 — User Registration & Roles

28. **Domain entities**: `User` (Id, Username, Email, PasswordHash, CreatedAt, UpdatedAt), `Role` (Id, Name), `UserRole` (UserId, RoleId — composite PK)
29. Update `PasswordEntry` — add `UserId` FK + `User` navigation property
30. **EF configurations**: `UserConfiguration` (unique indexes on Username/Email), `RoleConfiguration` (unique on Name, seed Admin + User), `UserRoleConfiguration` (composite key, relationships)
31. Update `PasswordEntryConfiguration` — add UserId FK with cascade delete
32. Update `AppDbContext` — add `DbSet<User>`, `DbSet<Role>`, `DbSet<UserRole>`, apply new configurations
33. `IUserRepository` interface — `GetByUsernameAsync`, `GetByEmailAsync`, `AddAsync`, `ExistsAsync`
34. `UserRepository` implementation — includes `UserRoles.Role` navigation in queries
35. `IPasswordHasher` interface — `Hash(password)`, `Verify(password, hash)`
36. `BCryptPasswordHasher` implementation using `BCrypt.Net-Next`
37. Update `DependencyInjection.cs` — register `IUserRepository`, `IPasswordHasher`, `AuthService`
38. `RegisterRequest` DTO (Username min 3, Email, Password min 8) + `RegisterResult` DTO (Success, Errors, UserId)
39. `AuthService` — `RegisterAsync` (validate uniqueness, hash password, assign "User" role), `ValidateCredentialsAsync` (replaces hardcoded check, returns User with roles)
40. Update `VaultService` — all methods accept `userId` parameter
41. Update `IVaultRepository` + `VaultRepository` — filter all queries by `UserId`
42. Update `AuthController` — add `POST /api/auth/register`, refactor login to use `AuthService`, include `ClaimTypes.Role` and `ClaimTypes.NameIdentifier` in JWT, remove hardcoded credentials
43. Update `VaultController` — extract UserId from JWT claims, pass to `VaultService`
44. `AdminController` — `POST /api/admin/users/{userId}/roles` with `[Authorize(Roles = "Admin")]`
45. **Seed data**: roles via EF `HasData`; default admin user via startup service (BCrypt hashing at runtime, credentials from `appsettings.json`)
46. Register page (`/register`): username + email + password + confirm password form
47. Update `PasswordApiClient` — add `RegisterAsync` method
48. Admin page (`/admin`): user table with promote/demote/delete actions, visible only to Admin role
49. `TokenService` — parse JWT to extract roles, expose `IsAdmin` property
50. NavMenu — show "Admin" link only when `TokenService.IsAdmin` is true
51. `AdminController` — add `GET /api/admin/users`, `DELETE /api/admin/users/{userId}`, `DELETE /api/admin/users/{userId}/roles/{roleName}`
52. `UserDto` — DTO for user list (Id, Username, Email, Roles, CreatedAt)
53. `PasswordApiClient` — add `GetUsersAsync`, `AssignRoleAsync`, `RemoveRoleAsync`, `DeleteUserAsync`
54. Delete confirmation — inline "Sure? [Yes] [No]" prompt before deleting a user

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
11. `POST /api/auth/register` with valid data → 200 with success result
12. Register with duplicate username or email → 400 with error message
13. Login with registered user → JWT contains `role` and `nameidentifier` claims
14. Vault isolation: User A's entries are not visible to User B
15. `POST /api/admin/users/{id}/roles` as Admin → 200; as User → 403
16. Login with seeded admin credentials → token contains "Admin" role
17. All new endpoints visible in Swagger UI
18. Admin page: list users with roles, promote/demote/delete actions work
19. Admin page: delete shows inline confirmation before executing
20. Admin page: seeded `admin` user cannot be demoted or deleted
21. Non-admin users see "Access denied" on `/admin`

## Decisions

- **Separate API + UI** — API is independently deployable and reusable
- **JWT auth** — multi-user with role claims (`ClaimTypes.Role`, `ClaimTypes.NameIdentifier`)
- **Custom User/Role entities** — no ASP.NET Identity; lightweight, full control
- **BCrypt password hashing** — industry standard with built-in salt and configurable work factor
- **Many-to-many User↔Role** — composite PK `(UserId, RoleId)` via `UserRole` join entity
- **Default "User" role on registration** — Admin-only escalation for additional roles
- **Per-user vault isolation** — `UserId` FK on `PasswordEntry`, cascade delete
- **Admin + User roles seeded** — roles via EF `HasData`; admin user via startup service (runtime BCrypt hashing)
- **Out of scope** — password reset, email confirmation, account lockout, user profile management
- **Clean Architecture** — 5 projects with strict dependency flow
- **Web references Application for DTOs only** — all data flows through the API
- **Encryption**: ASP.NET Core Data Protection API (machine-scoped keys by default)
- **Password generation**: `System.Security.Cryptography.RandomNumberGenerator` (cryptographically secure)

## Scope

**In scope**: generator, vault CRUD, encryption at rest, strength indicator, passphrase mode, JWT auth, Swagger, user registration, role-based auth, admin user management page

**Out of scope**: browser extension, import/export, categories/tags, password reset, email confirmation, account lockout
