# Lythar

> **Cartesian** to kryptonim projektu

Lythar to platforma społecznościowa skupiona na budowaniu autentycznych relacji w świecie rzeczywistym. W dobie cyfrowej izolacji, zachęcamy młodych ludzi do wychodzenia z domu i tworzenia lokalnych społeczności.

## Problem i rozwiązanie

Mimo nieograniczonych możliwości kontaktu online, coraz więcej młodych ludzi odczuwa samotność i społeczną izolację. Lythar odpowiada na ten problem poprzez:

- **Przełamywanie barier społecznych**: Ułatwiamy znajdowanie rówieśników o podobnych zainteresowaniach i organizowanie wspólnych aktywności
- **Promowanie lokalnych inicjatyw**: Dajemy narzędzia do tworzenia i odkrywania wydarzeń w najbliższej okolicy
- **Budowanie zaangażowanych społeczności**: Umożliwiamy tworzenie grup (kluby książki, drużyny sportowe, koła zainteresowań), które stają się fundamentem trwałych znajomości

## Główne Technologie

### Backend
- C#, .NET 10
- .NET Aspire 13
- ASP.NET Core
- Entity Framework Core
- PostgreSQL z PostGIS
- MinIO (S3-compatible storage)
- OpenAPI
- Docker

### Frontend
- TypeScript
- SvelteKit
- Svelte 5
- Tailwind CSS 4
- Zod 4
- Shadcn-Svelte
- MapBox
- Effect.TS
- Vercel AI SDK

## Funkcjonalność

1. **Zarządzanie kontem**: Tworzenie i personalizacja profili, zarządzanie danymi i hasłem
2. **Społeczności**: Zakładanie własnych społeczności (np. grupa biegowa, klub książki) lub dołączanie do istniejących. Narzędzia do moderacji, zarządzania członkami i uprawnieniami
3. **Czat**: Wiadomości w czasie rzeczywistym w kanałach publicznych (wewnątrz społeczności) i prywatnych rozmowach
4. **Wydarzenia**: Tworzenie wydarzeń, zarządzanie uczestnikami i określanie lokalizacji. Integracja z danymi geograficznymi (GeoJSON) ułatwia znajdowanie wydarzeń w okolicy
5. **Wyszukiwarka**: Szybkie odnajdywanie społeczności, wydarzeń i użytkowników dzięki funkcjonalności "tsvector" w PostgreSQL
6. **AI**: Sztuczna inteligencja wspiera poprawianie opisów wydarzeń

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
4.  W panelu Aspire, będzie prośba o podanie tokena MapBox.

### Deployment z Docker Compose

Wymagania:

- Docker
- Docker Compose

W głównym katalogu projektu uruchom następującą komendę, aby zbudować obrazy i uruchomić wszystkie usługi:

```bash
docker-compose up -d
```

Aplikacja będzie dostępna pod adresem `http://localhost:80`.