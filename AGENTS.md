# Cartesian Monorepo Guide

## Detailed Guides

- **Frontend**: See `Cartesian.Frontend/AGENTS.md` for SvelteKit/TypeScript conventions
- **Backend**: See `Cartesian.Services/AGENTS.md` for .NET/C# conventions

## Commands

- **Frontend** (`Cartesian.Frontend/`):
  - Build: `pnpm build`
  - Check/Typegen: `pnpm check` / `pnpm typegen`
  - Lint/Format: `pnpm lint` / `pnpm format` (Prettier)
  - _Note: No dedicated test runner currently configured._
- **Backend** (`Cartesian.AppHost/` etc.):
  - Build: `dotnet build Cartesian.slnx`
  - Run: `dotnet run --project Cartesian.AppHost`

## Code Style & Conventions

- **Frontend (SvelteKit + Tailwind + Effect.TS)**:
  - **Svelte 5**: MUST use Runes (`$state`, `$derived`, `$props`, `$effect`). No legacy syntax.
  - **Styling**: Tailwind CSS with `cn()` utility. Shadcn UI components.
  - **Logic**: Use `Effect.TS` for business logic. Avoid classes unless for State Machines.
  - **Formatting**: Prettier default. PascalCase for components, camelCase for props/functions.
  - **Full Details**: See `Cartesian.Frontend/AGENTS.md`
- **Backend (.NET)**: 
  - Follow idiomatic C# conventions with modern features (records, required, primary constructors)
  - IEndpoint pattern for all API endpoints
  - Entity/DTO separation with ToDto() methods
  - Typed errors inheriting from CartesianError
  - **Full Details**: See `Cartesian.Services/AGENTS.md`
- **General**:
  - **Strict Types**: No `any`. Define interfaces.
  - **Concise**: Minimal comments/prose. No emojis.
  - **Safety**: Verify imports/paths. Don't assume libraries exist.
