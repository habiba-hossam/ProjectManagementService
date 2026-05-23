# Project Management API

A scalable, production-ready **Project & Task Management REST API** built with **ASP.NET Core 9**, **Clean Architecture**, **CQRS + MediatR**, and **JWT Authentication**.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Setup (SQL Server)](#local-setup-sql-server)
  - [Docker Setup](#docker-setup)
- [API Endpoints](#api-endpoints)
- [Design Decisions](#design-decisions)
- [Bonus Features Implemented](#bonus-features-implemented)

---

## Architecture Overview

The solution follows **Clean Architecture** (also known as Onion Architecture), organized into three distinct layers with strict dependency rules:

```
┌─────────────────────────────────────────────┐
│                   API Layer                  │  ← Controllers, Middleware, DI wiring
│          (ProjectManagementAPI.API)          │
├─────────────────────────────────────────────┤
│            Infrastructure Layer             │  ← EF Core, JWT, BCrypt, Redis, Repos
│      (ProjectManagementAPI.Infrastructure)  │
├─────────────────────────────────────────────┤
│               Core / Domain Layer           │  ← Entities, Enums, Interfaces, CQRS
│           (ProjectManagementAPI.Core)       │
└─────────────────────────────────────────────┘
```

**Dependency Rule:** Inner layers define interfaces; outer layers implement them. The Core layer has zero dependencies on Infrastructure or API.

---

## Tech Stack

| Concern              | Technology                          |
|----------------------|-------------------------------------|
| Framework            | ASP.NET Core 9                      |
| ORM                  | Entity Framework Core 9             |
| Database             | SQL Server 2022                     |
| Authentication       | JWT Bearer Tokens                   |
| CQRS / Mediator      | MediatR 12                          |
| Validation           | FluentValidation 11                 |
| Password Hashing     | BCrypt.Net                          |
| Caching              | Redis (StackExchange.Redis)         |
| API Versioning       | Asp.Versioning                      |
| API Docs             | Swagger / OpenAPI                   |
| Unit Testing         | xUnit + Moq + FluentAssertions      |
| Containerization     | Docker + Docker Compose             |

---

## Project Structure

```
ProjectManagementAPI/
├── src/
│   ├── Core/                                  # Domain + Application logic
│   │   ├── Domain/
│   │   │   ├── Common/BaseEntity.cs           # Base entity with Id, CreatedAt, UpdatedAt
│   │   │   ├── Entities/                      # User, Project, ProjectTask
│   │   │   └── Enums/                         # TaskStatus, TaskPriority
│   │   └── Application/
│   │       ├── Common/
│   │       │   ├── Behaviors/                 # MediatR pipeline: ValidationBehavior
│   │       │   ├── Exceptions/                # NotFoundException, ConflictException, etc.
│   │       │   ├── Interfaces/                # IRepository<T>, IJwtService, ICacheService, ...
│   │       │   └── Models/                    # ApiResponse<T>, PaginatedList<T>
│   │       └── Features/
│   │           ├── Auth/Commands/             # Register, Login (Command + Validator + Handler)
│   │           ├── Projects/
│   │           │   ├── Commands/              # CreateProject, UpdateProject, DeleteProject
│   │           │   └── Queries/               # GetAllProjects, GetProjectById
│   │           └── Tasks/
│   │               ├── Commands/              # CreateTask, UpdateTaskStatus, DeleteTask
│   │               └── Queries/               # GetTasksByProject
│   │
│   ├── Infrastructure/                        # External concerns
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/                # EF Fluent API per entity
│   │   │   └── Migrations/                    # EF Core migration files
│   │   ├── Repositories/                      # Generic + specific repository implementations
│   │   └── Services/                          # JwtService, PasswordService, CacheService, CurrentUserService
│   │
│   └── API/                                   # Presentation layer
│       ├── Controllers/                       # AuthController, ProjectsController, TasksController
│       ├── Middleware/                        # GlobalExceptionMiddleware
│       ├── Extensions/                        # SwaggerExtensions (versioning setup)
│       └── Program.cs
│
└── tests/
    └── Application.UnitTests/                # xUnit tests with Moq + FluentAssertions
        └── Features/
            ├── Auth/                          # RegisterCommandHandlerTests
            ├── Projects/                      # CreateProject, DeleteProject tests
            └── Tasks/                         # CreateTask, UpdateTaskStatus tests
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server 2019+ (or Docker)
- Redis (optional — falls back to in-memory cache if not configured)
- Docker & Docker Compose (optional)

---

### Local Setup (SQL Server)

**1. Clone the repository**
```bash
git clone https://github.com/your-username/ProjectManagementAPI.git
cd ProjectManagementAPI
```

**2. Configure the connection string**

Edit `src/API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectManagementDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": ""
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ProjectManagementAPI",
    "Audience": "ProjectManagementAPIUsers",
    "ExpiryInMinutes": "60"
  }
}
```

**3. Apply database migrations**
```bash
cd src/Infrastructure
dotnet ef database update --startup-project ../API
```

Or from the solution root:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

**4. Run the API**
```bash
cd src/API
dotnet run
```

The API will be available at:
- **Swagger UI:** `http://localhost:5000` (default route)
- **API Base:** `http://localhost:5000/api/v1`

---

### Docker Setup

The fastest way to get everything running:

```bash
# Build and start all services (API + SQL Server + Redis)
docker-compose up --build

# Run in background
docker-compose up -d --build
```

Services started:
| Service    | Port  |
|------------|-------|
| API        | 5000  |
| SQL Server | 1433  |
| Redis      | 6379  |

Stop everything:
```bash
docker-compose down
```

---

### Run Unit Tests

```bash
dotnet test tests/Application.UnitTests
```

---

## API Endpoints

All protected endpoints require `Authorization: Bearer <token>` header.

### Authentication

| Method | Endpoint              | Auth | Description       |
|--------|-----------------------|------|-------------------|
| POST   | `/api/v1/auth/register` | ❌   | Register new user |
| POST   | `/api/v1/auth/login`    | ❌   | Login, get token  |

### Projects

| Method | Endpoint                      | Auth | Description            |
|--------|-------------------------------|------|------------------------|
| GET    | `/api/v1/projects`            | ✅   | Get all (paginated)    |
| GET    | `/api/v1/projects/{id}`       | ✅   | Get project by ID      |
| POST   | `/api/v1/projects`            | ✅   | Create project         |
| PUT    | `/api/v1/projects/{id}`       | ✅   | Update project         |
| DELETE | `/api/v1/projects/{id}`       | ✅   | Delete project         |

### Tasks

| Method | Endpoint                                          | Auth | Description            |
|--------|---------------------------------------------------|------|------------------------|
| GET    | `/api/v1/projects/{projectId}/tasks`              | ✅   | Get tasks (paginated)  |
| POST   | `/api/v1/projects/{projectId}/tasks`              | ✅   | Create task            |
| PATCH  | `/api/v1/projects/{projectId}/tasks/{id}/status`  | ✅   | Update task status     |
| DELETE | `/api/v1/projects/{projectId}/tasks/{id}`         | ✅   | Delete task            |

### Enum Values

**TaskStatus:** `0 = Todo`, `1 = InProgress`, `2 = Done`, `3 = Cancelled`

**TaskPriority:** `0 = Low`, `1 = Medium`, `2 = High`, `3 = Critical`

### Generic Response Format

All responses follow a consistent wrapper:
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... },
  "errors": null
}
```

---

## Design Decisions

### Clean Architecture + CQRS
Commands (write) and Queries (read) are strictly separated using MediatR. Each feature is self-contained in its own folder with Command/Query, Validator, and Handler — making features easy to locate, test, and modify independently.

### MediatR Pipeline Behavior
`ValidationBehavior<TRequest, TResponse>` intercepts every command before the handler runs. FluentValidation validators are auto-discovered and executed. If validation fails, a `ValidationException` is thrown and caught by `GlobalExceptionMiddleware`.

### Global Exception Handling
A single `GlobalExceptionMiddleware` maps all domain exceptions to consistent HTTP status codes and response shapes — no try/catch in controllers.

### Repository Pattern
A generic `IRepository<T>` provides base CRUD, extended by domain-specific interfaces (`IProjectRepository`, `ITaskRepository`) for query-specific methods. This keeps handlers decoupled from EF Core.

### Redis Caching
GET endpoints cache results with short TTLs. Cache is invalidated on mutations. The system gracefully falls back to in-memory cache if Redis is not configured — zero downtime impact.

### Security
- Passwords hashed with BCrypt (work factor 12)
- JWT tokens with configurable expiry
- Users can only access their own projects/tasks (enforced in handlers, not just controllers)
- `ClockSkew = TimeSpan.Zero` for precise token expiry

---

## Bonus Features Implemented

- ✅ **CQRS** — Commands and Queries fully separated
- ✅ **MediatR** — Pipeline with validation behavior
- ✅ **Docker** — Multi-stage Dockerfile + Docker Compose
- ✅ **Unit Tests** — xUnit + Moq + FluentAssertions
- ✅ **Redis** — Distributed caching with graceful fallback
- ✅ **Generic Response Wrapper** — `ApiResponse<T>` on every endpoint
- ✅ **API Versioning** — URL segment versioning (`/api/v1/`)
- ✅ **Pagination** — All list endpoints support `pageNumber` + `pageSize`
