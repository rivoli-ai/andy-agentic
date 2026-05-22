# Andy Agentic — Backend

ASP.NET Core 9 Web API that powers the Andy Agentic platform. Provides
agent/chat/tool/document/auth REST endpoints, an SSE chat stream, a SignalR
hub for RAG progress, and an MCP server surface so external MCP clients can
call our agents as tools.

> See `../ARCHITECTURE.md` for the system-level picture and
> `../IMPROVEMENT_PLAN.md` for the active roadmap. This README focuses on the
> Backend project specifically.

## Tech stack

- **.NET 9** (target framework `net9.0` in every `.csproj`)
- **ASP.NET Core** Web API + SignalR
- **EF Core 9** + **Npgsql** + **pgvector** (Microsoft.SemanticKernel.Connectors.PgVector)
- **Microsoft Semantic Kernel 1.74** for embeddings & prompt orchestration
- **ModelContextProtocol 1.2** — both as a server (we expose `/` and `/sse`)
  and as a client (calling user-configured external MCP servers)
- **OpenAI SDK 2.10**, plus an Ollama HTTP client
- **Mapster** (not AutoMapper) via `Mapster.DependencyInjection`
- **Swashbuckle** for Swagger UI / OpenAPI
- **Microsoft.Identity.Web** + `JwtBearer` for Microsoft Entra authentication
- **PdfPig**, **DocumentFormat.OpenXml**, **ClosedXML**, **QuestPDF** for
  document parse/export

## Solution layout (Clean Architecture)

```
Andy.Agentic.Domain         entities, models, repository/service interfaces
Andy.Agentic.Application    DTOs, use-case services (Agent/Chat/Tool/Doc/Auth)
Andy.Agentic.Infrastructure EF repos, migrations, LLM adapters, RAG, MCP transport
Andy.Agentic                ASP.NET host (Controllers, Mcps/, Program, Startup)
```

References go strictly inward. Domain has zero project references.

## Prerequisites

- .NET 9 SDK
- PostgreSQL 16 with the **pgvector** extension (or Docker — see below)
- An Azure AD tenant + app registrations (see `AUTHENTICATION_SETUP.md`)
- Node 20+ if you also want to run the Angular frontend in `../FrontEnd/`

## Run it

### Option A — Docker Compose (recommended)

```bash
cd Backend
docker-compose up -d        # brings up pgvector + the backend
docker-compose logs -f agentic-backend
```

Defaults from `.env` / `docker-compose.yml`:

- Postgres on host port `5432`
- Backend on host port `5000` → container port `80`
- Swagger UI: <http://localhost:5000/swagger>
- Health: <http://localhost:5000/api/health>

> Heads-up: the current healthcheck in `docker-compose.yml` probes `/health`
> rather than `/api/health`. This is tracked in `IMPROVEMENT_PLAN.md` §0.3.

### Option B — Local dev

```bash
cd Backend
dotnet restore
dotnet ef database update --project src/Andy.Agentic.Infrastructure
dotnet run --project src/Andy.Agentic
```

By default the host listens on ports 80 and 443 (see `Program.cs`). On macOS
you'll likely want to set `ASPNETCORE_URLS=http://localhost:5000` to avoid
the privileged-port issue:

```bash
ASPNETCORE_URLS=http://localhost:5000 dotnet run --project src/Andy.Agentic
```

### Configuration

`appsettings.json` and `appsettings.Development.json` hold defaults; secrets
and per-environment overrides go in environment variables
(`__` is the .NET section separator):

| Env var                                            | Notes                          |
|----------------------------------------------------|--------------------------------|
| `ConnectionStrings__DefaultConnection`             | Npgsql connection string       |
| `AzureAd__TenantId` / `__ClientId` / `__Audience`  | JWT bearer validation          |
| `ASPNETCORE_ENVIRONMENT`                           | `Development` / `Production`   |

The bundled defaults use `agentic_user` / `agentic_password` — fine for
local, **must be overridden in production**. See `POSTGRES_SETUP.md`.

## Authentication

Microsoft Entra (Azure AD) via JWT Bearer. The `AuthController`:

- `GET  /api/auth/me` — returns the current user, looked up by `oid` claim.
- `POST /api/auth/sync` — upserts a `UserEntity` from the JWT claims; called
  by the SPA on first login.

Two authorization policies are registered in `Startup.ConfigureAuthorization`:

- `ReadScope` — requires the `Api.Access` scope claim.
- `WriteRole` — requires the `Api.Write` role.

Full step-by-step setup (Azure portal app registrations, expose-an-API,
scopes, redirect URIs) is in `AUTHENTICATION_SETUP.md`.

## Key endpoints

The full surface is in Swagger; high-traffic ones:

| Method   | Route                              | Purpose                                  |
|----------|------------------------------------|------------------------------------------|
| `POST`   | `/api/chat/stream`                 | SSE: send message, stream tokens + tool calls |
| `GET`    | `/api/chat/sessions`               | List chat sessions for the current user  |
| `GET`    | `/api/chat/history/{agentId}`      | Get chat history for an agent            |
| `GET\|POST\|PUT\|DELETE` | `/api/agents[/{id}]`  | Manage agents                            |
| `GET\|POST\|PUT\|DELETE` | `/api/tools[/{id}]`   | Manage tools (REST + MCP definitions)    |
| `POST`   | `/api/tools/{id}/execute`          | Execute a tool out-of-band               |
| `GET\|POST\|PUT\|DELETE` | `/api/llm[/{id}]`     | Manage LLM provider configurations       |
| `POST`   | `/api/llm/test-connection`         | Validate an LLM config before saving     |
| `POST`   | `/api/documents`                   | Upload a document (kicks off RAG ingest) |
| `POST`   | `/api/exports/{type}`              | Export chat/document to PDF/DOCX/XLSX    |
| `GET`    | `/api/health` / `/api/health/detailed` | Liveness / detailed info             |
| `*`      | `/` and `/sse`                     | MCP server (Streamable HTTP + legacy SSE)|
| `*`      | `/documentRagHub`                  | SignalR hub for RAG progress             |

## Tests + coverage

```bash
dotnet test                                                                 # all
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" \
  -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
```

Tests live under `tests/`:

```
tests/Andy.Agentic.Tests/                ← controller-level
tests/Andy.Agentic.Application.Tests/    ← service-level
tests/Andy.Agentic.Domain.Tests/         ← pure domain
tests/Andy.Agentic.Infrastructure.Tests/ ← repos, tool factories
```

Coverage is currently low (especially around `ChatService` and
`ToolExecutionService`) — see `../IMPROVEMENT_PLAN.md` §3.3.

## Project structure

```
Backend/
├── Andy.Agentic.sln
├── Directory.Build.props
├── global.json
├── Dockerfile
├── docker-compose.yml
├── postgres/init/                       pgvector init scripts
├── docs/
│   └── CLEAN_ARCHITECTURE.md
├── src/
│   ├── Andy.Agentic/                    web host
│   │   ├── Controllers/                 (10 controllers)
│   │   ├── Mcps/AgentMcp.cs             MCP tools we expose
│   │   ├── Program.cs / Startup.cs
│   │   └── appsettings*.json
│   ├── Andy.Agentic.Application/
│   │   ├── DTOs/                        (request/response shapes)
│   │   ├── Services/                    Agent/Chat/Tool/Doc/Auth/LLM
│   │   ├── Interfaces/
│   │   └── Mapping/                     Mapster register
│   ├── Andy.Agentic.Domain/
│   │   ├── Entities/                    EF-mapped types
│   │   ├── Models/                      domain models (incl. Semantic/*)
│   │   ├── Queries/                     SearchCriteria + Pagination
│   │   └── Interfaces/
│   └── Andy.Agentic.Infrastructure/
│       ├── Data/                        AndyDbContext
│       ├── Migrations/                  EF Core migrations
│       ├── Mapping/                     entity ↔ domain Mapster registers
│       ├── Repositories/
│       │   ├── Database/                EF repos + DataSeeder
│       │   └── Llm/                     OpenAI + Ollama adapters
│       ├── Semantic/                    SK builder, RAG provider, hub, hosted service
│       │   ├── Builder/
│       │   ├── Interceptor/
│       │   ├── Provider/
│       │   └── Tools/                   API / MCP / Native tool factories
│       ├── Services/
│       │   ├── DatabaseService.cs
│       │   └── ToolProviders/           ApiToolProvider, McpToolProvider, McpService
│       └── UnitOfWorks/
└── tests/
```

## Conventions

- Public APIs use DTOs from `Andy.Agentic.Application/DTOs/`. Entities stay
  in `Andy.Agentic.Domain/Entities/` and are not serialised to clients.
- Mapping is **Mapster**, registered in `Application/Mapping/` and
  `Infrastructure/Mapping/`. Add new maps there, not inline.
- New repositories implement the relevant interface in
  `Andy.Agentic.Domain/Interfaces/Database/` and register in
  `Startup.ConfigureDatabaseRepositories`.
- New LLM providers implement `ILLmProviderRepository` and register in
  `Startup.ConfigureLlmRepositories`; the factory resolves by
  `LLMProviderType`.
- New tool sources implement `IToolProvider`; register in
  `Startup.ConfigureToolProviders`.
- Use `ILogger<T>` — do not add new `Console.WriteLine` calls. Existing ones
  are tracked for removal in `../IMPROVEMENT_PLAN.md` §2.2.

## License

Apache License 2.0 — see `LICENSE`.
