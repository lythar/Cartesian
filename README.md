# Cartesian

Cartesian to platforma społecznościowa oparta na tworzeniu lokalnych znajomości.

## Główne Technologie

- **Backend**: .NET 10, C#, ASP.NET Core
- **Orchestration**: .NET Aspire
- **Frontend**: SvelteKit, TypeScript, Tailwind CSS
- **Database**: PostgreSQL
- **Containerisation**: Docker, Docker Compose

## Uruchomienie

Projekt można uruchomić na dwa sposoby: lokalnie w trybie deweloperskim lub jako wdrożenie produkcyjne przy użyciu Docker Compose.

### Środowisko deweloperskie

Wymagania:

- .NET SDK 10
- Node.js v25
- pnpm (najlepiej przez [corepack](https://github.com/nodejs/corepack))

Kroki do uruchomienia:

1.  Sklonuj repozytorium.
2.  Zainstaluj zależności frontendu:
    ```bash
    cd Cartesian.Frontend
    pnpm install
    ```
3.  Uruchom aplikację z głównego katalogu projektu przy użyciu .NET Aspire:
    ```bash
    dotnet run --project Cartesian.AppHost
    ```
    Aspire automatycznie uruchomi wszystkie usługi. Panel Aspire będzie dostępny pod adresem wskazanym w konsoli.
4.  W panelu Aspire, będzie prośba o podanie tokena MapBox

### Deployment z Docker Compose

Wymagania:

- Docker
- Docker Compose

W głównym katalogu projektu uruchom następującą komendę, aby zbudować obrazy i uruchomić wszystkie usługi:

```bash
docker-compose up -d
```

Aplikacja będzie dostępna pod adresem `http://localhost:80`.
