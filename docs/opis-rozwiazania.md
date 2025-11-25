# Opis Rozwiązania

## 1. Wprowadzenie

Lythar (kryptonim: Cartesian) to platforma społecznościowa do budowania relacji w świecie rzeczywistym poprzez organizowanie lokalnych wydarzeń i tworzenie społeczności opartych na wspólnych zainteresowaniach.

## 2. Architektura systemu

### Frontend
- **SvelteKit** z **Svelte 5** (Runes)
- **Tailwind CSS 4** + **Shadcn-Svelte**
- **Effect.TS** dla logiki biznesowej
- **MapBox** dla map, **Vercel AI SDK** dla AI

### Backend
- **.NET 10** z **ASP.NET Core**
- **Entity Framework Core 9** + **PostgreSQL 18** z **PostGIS**
- **ASP.NET Core Identity** (cookie authentication)
- **MinIO** (S3-compatible storage)
- **FluentValidation**, **OpenAPI/Scalar**
- Wzorzec **IEndpoint** dla wszystkich API

### Infrastruktura
- **.NET Aspire 13** - orkiestracja środowiska deweloperskiego
- **Docker** + **Docker Compose** - konteneryzacja i deployment
- **PostgreSQL tsvector** - full-text search
- **PostGIS** - dane geoprzestrzenne

## 3. Funkcjonalność

- **Zarządzanie kontem**: Rejestracja, logowanie, avatary, zmiana hasła, blokowanie użytkowników
- **Społeczności**: Tworzenie społeczności, zarządzanie członkami, system ról i uprawnień (ManageEvents, ManageMembers, ModerateChat, ManageCommunity), bany
- **Czat**: Kanały publiczne, wiadomości prywatne (DM), reakcje, przypinanie wiadomości, moderacja, wyciszanie, SSE dla real-time
- **Wydarzenia**: Tworzenie wydarzeń z lokalizacją (GeoJSON), okna czasowe, tracking uczestników, ulubione, tagi, wsparcie AI dla opisów
- **Wyszukiwarka**: Full-text search (tsvector) dla społeczności, wydarzeń i użytkowników
- **Multimedia**: Upload plików do MinIO, avatary, załączniki (limit 10MB)

## 4. Bezpieczeństwo

- **Autentykacja**: ASP.NET Core Identity, cookie-based (7-dniowe sesje), wymagania hasła (min. 6 znaków, wielkie/małe litery, cyfry, znaki specjalne)
- **Autoryzacja**: Permission-based system, endpoint authorization, walidacja własności zasobów
- **Walidacja**: FluentValidation na wszystkich requestach, strong typing (TypeScript/C#), MIME type validation
- **Baza danych**: Parameterized queries (EF Core), migrations, transactions
- **Komunikacja**: HTTPS w produkcji, CORS policy, Svelte auto-escaping (no XSS)
- **Rate limiting**: Limity długości pól (usernames 3-30, descriptions max 2000), file size 10MB

## 5. Wydajność i skalowalność

- **Backend**: Async/await, DbContext pooling, query optimization, PostgreSQL indexes, tsvector indexes, SSE dla czatu
- **Frontend**: Svelte compilation, SSR/SSG, code splitting, Tailwind purging, lazy loading
- **Caching**: Browser caching, ETags, CDN-ready
- **Skalowalność**: Stateless API, database pooling, container-based, S3-compatible storage, PostgreSQL replication
- **Monitoring**: Aspire Dashboard, structured logging, distributed tracing, health checks

## 6. Uruchomienie

### Środowisko deweloperskie
**Wymagania:** .NET SDK 10, Node.js v25, pnpm

```bash
cd Cartesian.Frontend && pnpm install
cd .. && dotnet run --project Cartesian.AppHost
```

Aspire automatycznie uruchomi PostgreSQL (41021), MinIO (41022), backend i frontend (5173).

### Deployment produkcyjny
**Wymagania:** Docker, Docker Compose

```bash
docker-compose up -d
```

Aplikacja dostępna na `http://localhost:80`.

## 7. Zarządzanie

**Migracje bazy:**
```bash
dotnet ef migrations add <Nazwa> --project Cartesian.Services
dotnet ef database update --project Cartesian.Services
```

**Frontend:**
```bash
pnpm dev        # Development server
pnpm check      # Type checking
pnpm typegen    # Generowanie typów z OpenAPI
pnpm format     # Formatowanie
pnpm build      # Build produkcyjny
```

**Backend:**
```bash
dotnet build Cartesian.slnx
dotnet format
```

**API Docs:** Dostępne w dev na `/docs` (Scalar UI)

## 8. Wdrażanie

**Monitoring:**
- Dev: Aspire Dashboard (real-time logs, traces)
- Prod: Możliwość integracji z Grafana, Prometheus, Sentry

**Backup PostgreSQL:**
```bash
docker exec cartesian-postgres pg_dump -U postgres cartesian > backup.sql
```

**Skalowanie:**
- Backend: Stateless, można uruchomić wiele instancji z load balancerem
- Baza: PostgreSQL master-slave replication
- Storage: MinIO distributed mode lub migracja do S3

**Częste problemy:**
- Migracje nie działają: Sprawdź `AutoRunMigrations` lub uruchom ręcznie
- MinIO bucket: Automatycznie tworzony przy starcie
- CORS errors: Frontend musi działać na `localhost:5173`
- MapBox: Sprawdź `PUBLIC_MAPBOX_ACCESS_TOKEN`

## 9. Podsumowanie

Lythar to nowoczesna platforma społecznościowa zbudowana na .NET 10, Svelte 5 i PostgreSQL. Kluczowe elementy:

- **Architektura**: Endpoint Pattern, Entity/DTO separation, Aspire orchestration
- **Real-time**: SSE dla czatu, GeoJSON dla map, tsvector dla search
- **Bezpieczeństwo**: Identity, FluentValidation, permission-based access
- **DevEx**: Aspire Dashboard, hot reload, comprehensive API docs
- **Production-ready**: Docker Compose, migrations, health checks

Możliwości rozwoju: push notifications, AI recommendations, mobile apps, video chat, calendar integration.