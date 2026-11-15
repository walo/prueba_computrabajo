---
name: project-init
description: Enterprise project bootstrapping workflow for initializing full-stack .NET 10 Decoupled Onion and Angular 22+ Zoneless solutions from scratch, configuring dynamic PBAC security, session lifecycle (Login/Refresh/Logout/Me), mirror Unit + Integration tests, Jest, and Gitflow.
---

# Workflow: Project Initialization (`project-init.md`)

This workflow orchestrates the end-to-end bootstrapping of a new enterprise solution from scratch. It reads architecture governance from `CONSTITUTION.md`, extracts domain invariants from NotebookLM (or user-confirmed parameters when no notebook exists), and scaffolds a 100% ready-for-features backend, frontend, database, test suite, and CI pipeline.

---

## 1. Architectural Governance & Grounding

Before running any scaffolding commands, the agent **MUST**:

1. **Read Governance:** Read `.agents/CONSTITUTION.md` and `.agents/rules/endpoint-permissions-rbac.md` in the current workspace.
2. **Consult NotebookLM or Prompt User:** Query the official NotebookLM notebook via `notebook_query` (or ask the user if unconfigured) to extract:
   - Tenancy model: Multi-tenant B2B (`ITenantEntity`, PostgreSQL RLS) or Single-tenant B2C (`IUserOwnedEntity`).
   - Target clients of the system (_Web only, or Web + Mobile_).
   - Security level: Level 3 PBAC (`[HasPermission]` on backend + `*ngxPermissionsOnly` and `permissionGuard` on frontend).
3. **Seal Architecture Profile in `PROJECT_PROFILE.md`:**  
   Immediately create/update `PROJECT_PROFILE.md` at the root of the target project (never modify `.agents/CONSTITUTION.md`, which is shared across projects via `agents-base`), recording the active decisions:
   - Tenancy: `[x] Single-Tenant` or `[x] Multi-Tenant`
   - Target Clients: `[x] Web (Angular)` and `[x]` or `[ ]` for `Mobile`
   - Security Level: `[x] PBAC (Permission-Based Access Control)`
   *(This ensures that all subsequent workflows like `spec-generation` and `feature-scaffold` read the project-specific profile without mutating `agents-base`).*

---

## 2. Execution Steps

### Step 0: Project Profile Sealing in `PROJECT_PROFILE.md`
Confirm and write `PROJECT_PROFILE.md` at the project root. If Single-Tenant, scaffold standard User-Centric schemas without RLS. If Multi-Tenant, configure `TenantId` and RLS policies.

### Step 1: Environment, Secrets & Local Docker Infrastructure

6. Verify `.env.example` exists; copy to `.env` if missing and configure local credentials.
7. Verify `.gitignore` excludes `.env`, `dist/`, `bin/`, `obj/`, `node_modules/`, `graphify-out/cache/`, `.agents/`, and `AGENTS.md` (since rules and skills are linked from `../agents-base` instead of duplicated per repo).
8. Create `link-agents.ps1` at the project root so any developer cloning the repository links `.agents/` and `AGENTS.md` to the sibling `../agents-base` in one command (without duplicating skills across projects), and run it immediately:
   ```powershell
   $base = Join-Path $PSScriptRoot "..\agents-base"
   if (-not (Test-Path $base)) { Write-Error "No se encontró ../agents-base"; exit 1 }
   if (-not (Test-Path "$PSScriptRoot\.agents")) { cmd /c mklink /J "$PSScriptRoot\.agents" "$base\.agents" }
   if (-not (Test-Path "$PSScriptRoot\AGENTS.md")) { cmd /c mklink /H "$PSScriptRoot\AGENTS.md" "$base\AGENTS.md" }
   ```
   And add a **"Configuración Inicial del Desarrollador"** section at the top of the project's `README.md` instructing teammates to clone `../agents-base` and `../ui-design-system` as siblings and run `powershell -ExecutionPolicy Bypass -File .\link-agents.ps1` once after cloning.
9. Scaffold `docker-compose.yml` (PostgreSQL 16 Alpine + Redis 7 Alpine) and start containers:
   ```bash
   docker compose up -d
   ```


### Step 2: Backend Solution Scaffolding (.NET 10 Decoupled Onion Architecture)

Invoke skill: **`be-dotnet-scaffold`**

3. Create classic `.sln` solution (`dotnet new sln -n <ProjectName> --format sln`) inside `src/backend/` with strict **1 public type per physical `.cs` file** and clean Solution Folders:
   - `Core`: `Domain` (0 external dependencies) and `Application` (CQRS MediatR, FluentValidation, `ErrorOr`, Finders).
   - `Infrastructure` (**4 decoupled Class Libraries**, each referencing only `Application`):
     - `Authentication` (`JwtProvider`, `PasswordHasher`, `TokenBlacklistService`, `PermissionCacheService`)
     - `ExternalServices` (`HttpClient` integrations)
     - `Persistence` (`AppDbContext` with `ValueGeneratedNever()` fluent configurations, Repositories, `.AsNoTracking()` Finders, Migrations, `InitialDataSeeder`)
     - `Shared` (`CacheService` with Redis + `IMemoryCache` fallback, `DateTimeProvider`)
   - `Presentation`: `Web.API` (`GlobalExceptionHandlingMiddleware`, `JwtBlacklistMiddleware`, `PermissionAuthorizationMiddleware`, `HasPermissionAttribute`, HealthChecks `/health` & `/health/ready`, `AuthController` [`login`, `refresh`, `logout`, `me`], `UsersController` [`GET /api/v1/users`], and `public partial class Program;`).
   - `Tests`: `Tests.UnitTests` (mirror folder hierarchy) and `Tests.IntegrationTests` (`CustomWebApplicationFactory : WebApplicationFactory<Program>`).


### Step 3: Database Migration, Seeding & Backend Test Verification

Invoke skills: **`be-database-sp`**, **`be-dotnet-scaffold`**, and **`be-dotnet-test`**

1. Generate and apply initial migration in `src/Infrastructure/Persistence`:
   ```bash
   dotnet ef migrations add InitialAuthMigration --project src/backend/src/Infrastructure/Persistence --startup-project src/backend/src/Presentation/Web.API
   dotnet ef database update --project src/backend/src/Infrastructure/Persistence --startup-project src/backend/src/Presentation/Web.API
   ```
4. Configure PostgreSQL Row Level Security (RLS) policies if multi-tenant.
5. Verify `InitialDataSeeder` seeds `Permissions.All`, `Admin` & `User` roles, and initial admin user.
6. Author mirror Unit Tests and `AuthFlowIntegrationTests` (verifying `/health`, `/auth/login`, `/auth/me`, `/auth/refresh` RTR + reuse detection, `/auth/logout` JTI blacklist, and PBAC `403 Forbidden`), then verify 0 warnings / 0 errors:
   ```bash
   dotnet build src/backend/<ProjectName>.sln
   dotnet test src/backend/<ProjectName>.sln
   ```

### Step 4: Frontend Scaffolding (Angular 22+ Zoneless, `@accionadev/ui-kit`, Dynamic PBAC & Jest)

Invoke skills: **`fe-angular-scaffold`** and **`fe-angular-test`**

1. Scaffold Standalone Angular 22+ application inside `src/frontend/`:
   - Zoneless change detection (`provideZonelessChangeDetection()`).
   - Jest Zoneless configuration (`jest-preset-angular` with `setupZonelessTestEnv()` and `moduleNameMapper` for `@accionadev/ui-kit` + single-instance `@angular/*` via `require.resolve`), `npm run test:ci`.
   - Install runtime packages `ngx-permissions` and `lucide` (`npm install ngx-permissions lucide --legacy-peer-deps`) required by PBAC and `<ad-icon>`.
   - Connect to the decoupled Design System (`../ui-design-system`) enforcing single-instance `@angular/core` (prevents `NG0203`):
     - Configure `tsconfig.json` `paths` (without deprecated `baseUrl`) with `"@accionadev/ui-kit": ["./../../../ui-design-system/projects/ui-kit/src/public-api.ts"]`, `"@angular/*": ["./node_modules/@angular/*"]`, `"rxjs": ["./node_modules/rxjs"]`, `"rxjs/*": ["./node_modules/rxjs/*"]`.
     - Configure `package.json` `"start": "ng serve --prebundle=false"` so Vite dev server does not pre-bundle a duplicate `@angular/core`.
     - Configure `tailwind.config.js` inheriting `presets: [require('../../../ui-design-system/tailwind.preset.js')]` and scanning `../../../ui-design-system/projects/ui-kit/src/**/*.{html,ts}`.
     - Configure `src/styles.css` by copying `.agents/skills/fe-angular-scaffold/examples/cascaron/styles.example.css` verbatim (all `--ad-*` tokens including `--ad-primary-foreground`, `--ad-text`, `--ad-text-muted`, `--ad-border-strong`, and `.dark` overrides). Only change `--ad-primary*` and `--ad-ring` RGB channels if the client requires a different brand color.
   - **Mandatory Verbatim Cascaron Copy (NEVER rewrite UI templates from scratch):** Read and copy the canonical templates from `.agents/skills/fe-angular-scaffold/examples/cascaron/`:
     - `environments/environment.ts` & `environment.prod.ts` from `environment.example.ts` (including `appName`, `appSubtitle`, `logoUrl`, `logoIcon`, `companyName`, `version`).
     - Core session & PBAC infrastructure: `StorageService`, `AuthService`, `LoaderService` + `loaderInterceptor`, `NotificationService` (with `success`, `error`, `info`, `warning`), `tokenInterceptor` (with Silent Refresh Token Rotation queue on `401`), `authGuard`, `publicGuard`, `permissionGuard`, and `provideAppInitializer(() => inject(AuthService).initializeSession())` hydrating permissions via `GET /api/v1/auth/me`.
     - Transversal `StatusPageComponent` (`shared/pages/status-page/`), `GlobalLoaderComponent`, and top-right `ToastContainerComponent` (`fixed top-20 right-6 z-50`) copied from `examples/cascaron/`.
     - `src/app/auth/login/login.component.ts` copied from `login.component.example.ts` (MUST include `<ad-brand-logo size="lg" />` bound to `environment`, `<ad-card>`, `<ad-alert>`, `<ad-input>`, and `<ad-button [fullWidth]="true">`).
     - `src/app/layout/shell/shell.component.ts` copied from `shell.component.example.ts` (MUST wrap `<ad-app-shell>` inside `<div class="h-screen w-screen overflow-hidden">` so the footer stays pinned to the bottom, and MUST bind `[navItems]`, `[pageTitle]`, `[logoUrl]`, `[logoIcon]`, `[companyName]`, `[version]`, and `(logoutClick)`).
     - `src/app/features/dashboard/dashboard.component.ts` copied from `dashboard.component.example.ts` with KPI `<ad-card>`s and `<ad-badge>`s.
   ```
3. Verify production bundle and Jest unit tests pass:
   ```bash
   cd src/frontend && npm run build && npm run test:ci
   ```

### Step 5: Mobile Scaffolding (Conditional)

_If NotebookLM or user requirements specify a Mobile App:_

3. Scaffold mobile workspace inside `src/mobile/` consuming the same REST contracts.

### Step 6: CI/CD Pipeline & Gitflow Initialization

Invoke skill: **`ops-github-gitflow`**

5. Scaffold `.github/workflows/ci.yml` running parallel `.NET 10` (`dotnet build` + `dotnet test` on `.sln`) and `Angular 22` (`npm ci`, `npm run test:ci`, `npm run build`) jobs.
6. Initialize Git repository with `main` and `develop` branches and commit cleanly on `develop`.

### Step 7: Knowledge Graph Initialization

Invoke skill: **`graphify`**

1. Update the project knowledge graph:
   ```bash
   graphify update .
   ```

---

## 3. Quality Gate Checklist

- [ ] `.agents/CONSTITUTION.md` & `.agents/rules/endpoint-permissions-rbac.md` strictly applied.
- [ ] Classic `.sln` file with 4 decoupled `Infrastructure` class libraries (`Authentication`, `ExternalServices`, `Persistence`, `Shared`) and 1 public type per `.cs` file.
- [ ] Base endpoints operational: `/auth/login`, `/auth/refresh` (RTR), `/auth/logout` (Blacklist), `/auth/me` (permissions), `/users` (`[HasPermission]`), `/health`.
- [ ] Backend compiles with 0 errors / 0 warnings, and both `Tests.UnitTests` (mirror hierarchy) and `Tests.IntegrationTests` (`WebApplicationFactory`) pass.
- [ ] Frontend Zoneless compiles cleanly (`npm run build`), Jest tests pass (`npm run test:ci`), `provideAppInitializer` hydrates `/auth/me` on F5, and transversal `StatusPageComponent` covers `/forbidden` (403) and `**` (404).
- [ ] `.github/workflows/ci.yml` configured and `graphify update .` executed.
