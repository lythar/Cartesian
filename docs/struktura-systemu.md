# Struktura Systemu

## Monorepo

Projekt Lythar (Cartesian) wykorzystuje architekturę **monorepo** - wszystkie komponenty systemu (frontend, backend, dokumentacja) znajdują się w jednym repozytorium. Zapewnia to spójność wersji, łatwiejsze zarządzanie zależnościami i uproszczone deployment.

## Główne Katalogi

```
Cartesian/
├── Cartesian.Frontend/     # Aplikacja frontendowa (SvelteKit)
├── Cartesian.Services/     # API backendu (ASP.NET Core)
├── Cartesian.AppHost/      # Orkiestracja Aspire
├── Cartesian.ServiceDefaults/  # Współdzielona konfiguracja Aspire
├── docs/                   # Dokumentacja VitePress
├── docker-compose.yaml     # Deployment produkcyjny
└── Cartesian.slnx          # Solution .NET
```

## Cartesian.Frontend/

Aplikacja frontendowa zbudowana w **SvelteKit** z **Svelte 5**.

```
Cartesian.Frontend/
├── src/
│   ├── routes/           # File-based routing SvelteKit
│   │   ├── (app)/        # Authenticated routes
│   │   ├── (auth)/       # Public auth routes
│   │   └── ...
│   ├── lib/
│   │   ├── api/          # API client layer
│   │   │   ├── queries/
│   │   │   ├── cartesian-client.ts  # Auto-generated (OpenAPI)
│   │   │   └── ...
│   │   ├── components/   # Komponenty UI
│   │   │   ├── auth/
│   │   │   ├── chat/
│   │   │   ├── community/
│   │   │   ├── map/
│   │   │   ├── profile/
│   │   │   ├── layout/
│   │   │   └── ui/       # Shadcn-Svelte
│   │   ├── constants/
│   │   ├── context/
│   │   ├── effects/      # Effect.TS business logic
│   │   │   ├── schemas/
│   │   │   ├── services/
│   │   │   └── utils/
│   │   ├── hooks/
│   │   ├── material-gen/
│   │   ├── paraglide/    # i18n (auto-generated)
│   │   ├── stores/       # Global state (Svelte 5 runes)
│   │   │   └── ...
│   │   └── utils/
│   ├── app.html
│   ├── app.css
│   ├── hooks.server.ts
│   └── hooks.ts
├── static/
├── messages/             # i18n translations
├── project.inlang/
├── svelte.config.js
├── vite.config.ts
├── orval.config.ts       # OpenAPI client generation
├── components.json
├── tsconfig.json
├── package.json
└── Dockerfile
```

## Cartesian.Services/

Backend API zbudowany w **ASP.NET Core** z **.NET 10**.

```
Cartesian.Services/
├── Account/               # Autentykacja i zarządzanie użytkownikami
│   ├── CartesianUser.cs   # Model użytkownika (Identity)
│   ├── LoginEndpoint.cs   # Endpoint logowania
│   ├── RegisterEndpoint.cs # Endpoint rejestracji
│   └── ...
├── Communities/           # Zarządzanie społecznościami
│   ├── Community.cs       # Encja społeczności
│   ├── Membership.cs      # Członkostwo w społeczności
│   └── ...
├── Events/                # Wydarzenia lokalne
│   ├── Event.cs           # Encja wydarzenia
│   ├── EventWindow.cs     # Okna czasowe wydarzeń
│   └── ...
├── Chat/                  # System czatu
│   ├── ChatMessage.cs     # Wiadomości
│   ├── ChatSseService.cs  # Server-Sent Events
│   └── ...
├── Content/               # Upload mediów (MinIO)
│   ├── Media.cs           # Encja pliku
│   ├── MediaService.cs    # Logika uploadu
│   └── ...
├── Database/              # Baza danych
│   ├── CartesianDbContext.cs  # DbContext EF Core
│   └── Migrations/        # Migracje bazy danych
├── Endpoints/             # Infrastruktura endpointów
│   ├── IEndpoint.cs       # Interfejs dla wszystkich endpointów
│   ├── CartesianError.cs  # Bazowa klasa błędów
│   └── ...
├── Search/                # Wyszukiwarka full-text
└── Program.cs             # Punkt wejścia aplikacji
```

**Kluczowe pliki:**
- Każdy `*Endpoint.cs` - Endpoint API implementujący `IEndpoint`
- Każdy `*.cs` w folderze domeny - Entity lub DTO
- `Database/Migrations/` - Automatycznie generowane migracje
- `Program.cs` - Konfiguracja serwisów, middleware, Identity

## Cartesian.AppHost/

Orkiestracja środowiska deweloperskiego z **.NET Aspire**.

```
Cartesian.AppHost/
├── AppHost.cs             # Definicja zasobów Aspire
├── appsettings.json       # Konfiguracja
└── Cartesian.AppHost.csproj
```

**Co robi:**
- Uruchamia PostgreSQL z PostGIS
- Uruchamia MinIO (storage)
- Uruchamia backend (Cartesian.Services)
- Uruchamia frontend (pnpm dev)
- Udostępnia Aspire Dashboard (monitoring, logs, traces)

## Cartesian.ServiceDefaults/

Współdzielona konfiguracja Aspire dla telemetrii, health checks i service discovery.

```
Cartesian.ServiceDefaults/
├── Extensions.cs          # Extension methods dla Aspire
└── Cartesian.ServiceDefaults.csproj
```

## docs/

Dokumentacja projektu w **VitePress**.

```
docs/
├── .vitepress/
│   └── config.ts          # Konfiguracja VitePress
├── opis-systemu/
│   ├── struktura-systemu.md
│   └── wykaz-elementow.md
├── index.md               # Strona główna docs
├── main.md                # Informacje o projekcie
└── opis-rozwiazania.md    # Szczegółowy opis rozwiązania
```

## Pliki Konfiguracyjne Root

- **docker-compose.yaml** - Deployment produkcyjny (Caddy, PostgreSQL, MinIO, backend, frontend)
- **Caddyfile** - Konfiguracja reverse proxy Caddy
- **Cartesian.slnx** - Solution .NET (wszystkie projekty C#)
- **global.json** - Wersja .NET SDK (10.0)
- **.editorconfig** - Style kodu dla C#
- **.dotnet-format.json** - Konfiguracja formatowania .NET
- **package.json** - Zależności root (husky, lint-staged)

## Wzorce Organizacji Kodu

### Frontend (SvelteKit)
- **Komponenty**: PascalCase, pliki `kebab-case.svelte`
- **Routes**: File-based routing w `src/routes/`
- **State**: Klasy z `$state` w `*.svelte.ts`
- **Logika**: Effect.TS w `lib/`

### Backend (.NET)
- **Endpoints**: Wzorzec `IEndpoint`, każdy w osobnym pliku
- **Entities**: Klasy w folderach domenowych
- **DTOs**: Records w tym samym pliku co entity
- **Errors**: Dziedziczą z `CartesianError`
- **Validation**: FluentValidation w `*Validator.cs`

## Komunikacja Między Komponentami

```
Frontend (SvelteKit)
    ↓ HTTP/HTTPS
Backend (ASP.NET Core)
    ↓ EF Core
PostgreSQL + PostGIS

Backend (ASP.NET Core)
    ↓ S3 API
MinIO (Storage)

Frontend ←→ Backend
    SSE (Server-Sent Events) dla czatu
    Cookie authentication dla sesji
```

## Deployment

**Development**: `dotnet run --project Cartesian.AppHost` (Aspire orchestration)

**Production**: `docker-compose up -d` (wszystkie serwisy w kontenerach)