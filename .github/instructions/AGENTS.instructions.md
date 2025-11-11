---
applyTo: "**"
---

You always use the latest stable versions of SvelteKit, Tailwind, and TypeScript, and you are familiar with the latest features and best practices.

You carefully provide accurate, factual, thoughtful answers, and are a genius at reasoning.

General preferences:

- Follow the user's requirements carefully & to the letter
- Always write correct, up-to-date, bug-free, fully functional and working, secure, performant and efficient code
- Focus on readability over being performant
- Fully implement all requested functionality
- Leave NO todos, placeholders or missing pieces in the code
- Be sure to reference file names
- Be concise. Minimize any other prose
- If you think there might not be a correct answer, you say so. If you do not know the answer, say so instead of guessing
- Never write comments explaining what the line does unless expcliticly given permission by the user. If you need to, add why? comments.
- Never use stupid emojis. They're not needed. When you need an icon. Use `@lucide/svelte`. No emojis in code / logs directly.
- Absolutely do not create any .MD documentations of the work you've just created.
- Keep your message response small and concise - like talking to a chill bro instead of a lot of yapping.

Key Principles

- Write concise, technical code with accurate Svelte 5 and SvelteKit examples.
- Leverage SvelteKit's server-side rendering (SSR) and static site generation (SSG) capabilities.
- Prioritize performance optimization and minimal JavaScript for optimal user experience.
- Use descriptive variable names and follow Svelte and SvelteKit conventions.
- Organize files using SvelteKit's file-based routing system.
- Use Effect.TS for any functionality regarding logic.

Code Style and Structure

- Write concise, technical TypeScript or JavaScript code with accurate examples.
- Use functional and declarative programming patterns; avoid unnecessary classes except for state machines.
- Prefer iteration and modularization over code duplication.
- Structure files: component logic, markup, styles, helpers, types.
- Use Effect.TS for any logic.
- Follow Svelte's official documentation for setup and configuration: https://svelte.dev/docs
- Follow Effect.TS LLM's txt for best practices and functions.

Naming Conventions

- Use lowercase with hyphens for component files (e.g., `components/auth-form.svelte`).
- Use PascalCase for component names in imports and usage.
- Use camelCase for variables, functions, and props.

TypeScript Usage

- Use TypeScript for all code; prefer interfaces over types.
- Avoid enums; use const objects instead.
- Use functional components with TypeScript interfaces for props.
- Enable strict mode in TypeScript for better type safety.

Svelte Runes

- `$state`: Declare reactive state
  ```typescript
  let count = $state(0);
  ```
- `$derived`: Compute derived values
  ```typescript
  let doubled = $derived(count * 2);
  ```
- `$effect`: Manage side effects and lifecycle
  ```typescript
  $effect(() => {
    console.log(`Count is now ${count}`);
  });
  ```
- `$props`: Declare component props
  ```typescript
  let { optionalProp = 42, requiredProp } = $props();
  ```
- `$bindable`: Create two-way bindable props
  ```typescript
  let { bindableProp = $bindable() } = $props();
  ```
- `$inspect`: Debug reactive state (development only)
  ```typescript
  $inspect(count);
  ```

UI and Styling

- Use Tailwind CSS for utility-first styling approach.
- Leverage Shadcn components for pre-built, customizable UI elements.
- Import Shadcn components from `@repo/ui`.
- When modifying Shadcn components, do that in `@repo/ui`
- Organize Tailwind classes using the `cn()` utility from `@repo/ui`.
- Use Svelte's built-in transition and animation features.

Shadcn Color Conventions

- Use `background` and `foreground` convention for colors.
- Define CSS variables without color space function:
  ```css
  --primary: 222.2 47.4% 11.2%;
  --primary-foreground: 210 40% 98%;
  ```
- Usage example:
  ```svelte
  <div class="bg-primary text-primary-foreground">Hello</div>
  ```
- Key color variables:
  - `--background`, `--foreground`: Default body colors
  - `--muted`, `--muted-foreground`: Muted backgrounds
  - `--card`, `--card-foreground`: Card backgrounds
  - `--popover`, `--popover-foreground`: Popover backgrounds
  - `--border`: Default border color
  - `--input`: Input border color
  - `--primary`, `--primary-foreground`: Primary button colors
  - `--secondary`, `--secondary-foreground`: Secondary button colors
  - `--accent`, `--accent-foreground`: Accent colors
  - `--destructive`, `--destructive-foreground`: Destructive action colors
  - `--ring`: Focus ring color
  - `--radius`: Border radius for components

SvelteKit Project Structure

- Use the recommended SvelteKit project structure:
  ```
  - src/
    - lib/
    - routes/
    - app.html
  - static/
  - svelte.config.js
  - vite.config.js
  ```

Component Development

- Create .svelte files for Svelte components.
- Use .svelte.ts files for component logic and state machines.
- Implement proper component composition and reusability.
- Use Svelte's props for data passing.
- Leverage Svelte's reactive declarations for local state management.

State Management

- Use runed FSM's when absolutely needed.
- Use classes for complex state management (state machines):

  ```typescript
  // counter.svelte.ts
  class Counter {
    count = $state(0);
    incrementor = $state(1);

    increment() {
      this.count += this.incrementor;
    }

    resetCount() {
      this.count = 0;
    }

    resetIncrementor() {
      this.incrementor = 1;
    }
  }

  export const counter = new Counter();
  ```

- Use in components:

  ```svelte
  <script lang="ts">
  	import { counter } from "./counter.svelte.ts";
  </script>

  <button onclick={() => counter.increment()}>
  	Count: {counter.count}
  </button>
  ```

Routing and Pages

- Utilize SvelteKit's file-based routing system in the src/routes/ directory.
- Implement dynamic routes using [slug] syntax.
- Use load functions for server-side data fetching and pre-rendering.
- Implement proper error handling with +error.svelte pages.

Server-Side Rendering (SSR) and Static Site Generation (SSG)

- Leverage SvelteKit's SSR capabilities for dynamic content.
- Implement SSG for static pages using prerender option.
- Use the adapter-auto for automatic deployment configuration.

Performance Optimization

- Leverage Svelte's compile-time optimizations.
- Use `{#key}` blocks to force re-rendering of components when needed.
- Implement code splitting using dynamic imports for large applications.
- Profile and monitor performance using browser developer tools.
- Use `$effect.tracking()` to optimize effect dependencies.
- Minimize use of client-side JavaScript; leverage SvelteKit's SSR and SSG.
- Implement proper lazy loading for images and other assets.

Data Fetching and API Routes

- Use load functions for server-side data fetching.
- Implement proper error handling for data fetching operations.
- Create API routes in the src/routes/api/ directory.
- Implement proper request handling and response formatting in API routes.
- Use SvelteKit's hooks for global API middleware.

Accessibility

- Ensure proper semantic HTML structure in Svelte components.
- Ensure keyboard navigation support for interactive elements.
- Use Svelte's bind:this for managing focus programmatically.

Key Conventions

1. Embrace Svelte's simplicity and avoid over-engineering solutions.
2. Use SvelteKit for full-stack applications with SSR and API routes.
3. Prioritize Web Vitals (LCP, FID, CLS) for performance optimization.
4. Use environment variables for configuration management.
5. Follow Svelte's best practices for component composition and state management.
   w6. Ensure cross-browser compatibility by testing on multiple platforms.
6. Keep your Svelte and SvelteKit versions up to date.

Documentation

- Svelte 5 Runes: https://svelte-5-preview.vercel.app/docs/runes
- Svelte Documentation: https://svelte.dev/docs
- SvelteKit Documentation: https://kit.svelte.dev/docs
- Effect.TS Documentation: https://effect.website/llms-full.txt

Refer to Svelte, SvelteKit, and documentation for detailed information on components, and best practices.

Refer to the Effect.TS LLM.txt file for the best practices and methods regarding Effect.TS

You are able to use the Svelte MCP server, where you have access to comprehensive Svelte 5 and SvelteKit documentation. Here's how to use the available tools effectively:

## Available MCP Tools:

### 1. list-sections

Use this FIRST to discover all available documentation sections. Returns a structured list with titles, use_cases, and paths.
When asked about Svelte or SvelteKit topics, ALWAYS use this tool at the start of the chat to find relevant sections.

### 2. get-documentation

Retrieves full documentation content for specific sections. Accepts single or multiple sections.
After calling the list-sections tool, you MUST analyze the returned documentation sections (especially the use_cases field) and then use the get-documentation tool to fetch ALL documentation sections that are relevant for the user's task.

### 3. svelte-autofixer

Analyzes Svelte code and returns issues and suggestions.
You MUST use this tool whenever writing Svelte code before sending it to the user. Keep calling it until no issues or suggestions are returned.
