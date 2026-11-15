# Redarbor Product Inventory Management System

Full-stack enterprise solution built with **.NET 10 (Decoupled Onion Architecture)**, **CQRS (MediatR)**, **Entity Framework Core (Read)** + **Dapper (Write)** over **SQL Server 2022 (Zero Foreign Keys)**, **OAuth2/JWT + PBAC Security**, **Redis 7**, and **Angular 22+ Zoneless** connected to **`@accionadev/ui-kit`**.

---

## Configuracion Inicial del Desarrollador

1. **Clonar dependencias hermanas (si aplica entorno multi-repo):**
   ```powershell
   # Clonar ../agents-base y ../ui-design-system como carpetas hermanas
   powershell -ExecutionPolicy Bypass -File .\link-agents.ps1
   ```
2. **Variables de Entorno:**
   ```powershell
   Copy-Item .env.example .env
   ```
3. **Levantar Infraestructura Docker (SQL Server 2022 + Redis 7):**
   ```powershell
   docker compose up -d
   docker compose ps
   ```
4. **Ejecutar Backend (.NET 10 + Swagger):**
   ```powershell
   dotnet restore src/backend/EnterpriseApp.sln
   dotnet build src/backend/EnterpriseApp.sln
   dotnet test src/backend/EnterpriseApp.sln
   dotnet run --project src/backend/src/Presentation/Web.API/Web.API.csproj
   ```
   - Swagger UI disponible en `http://localhost:5000/swagger` (o el puerto configurado en `launchSettings.json`).
   - Health checks en `GET /health` y `GET /health/ready`.
5. **Ejecutar Frontend (Angular 22+ Zoneless + Jest):**
   ```powershell
   cd src/frontend
   npm install --legacy-peer-deps
   npm run test:ci
   npm run build
   npm start
   ```

---

## Architecture Overview

- **Backend (`src/backend/EnterpriseApp.sln`):**
  - `Core/Domain`: Pure domain primitives, entities, domain events, and permission constants (0 external infra dependencies).
  - `Core/Application`: CQRS commands, queries, FluentValidation validators, `ErrorOr<T>` result pattern, and interface abstractions.
  - `Infrastructure/Persistence`: SQL Server 2022 `AppDbContext` (**EF Core `.AsNoTracking()`** for `IFinder` reads), **Dapper** repositories for writes, and **zero Foreign Key constraints** (`FOREIGN KEY`).
  - `Infrastructure/Authentication`: Ultralight OAuth2/JWT token provider, BCrypt password hashing, Redis/Memory JTI blacklist, and PBAC permission cache.
  - `Infrastructure/Shared`: Distributed Redis cache with `IMemoryCache` fallback and UTC date/time abstraction.
  - `Infrastructure/ExternalServices`: Outbound HTTP client infrastructure.
  - `Presentation/Web.API`: RESTful controllers with `[HasPermission(...)]`, RFC 7807 `ProblemDetails` global exception handling, JTI blacklist middleware, PBAC authorization middleware, HealthChecks, and Swagger/OpenAPI.
- **Frontend (`src/frontend/`):**
  - Angular 22+ Standalone Zoneless (`provideZonelessChangeDetection()`), Signals, `ChangeDetectionStrategy.OnPush`, `provideAppInitializer` session hydration via `/api/v1/auth/me`, Silent Refresh Token Rotation (`tokenInterceptor`), PBAC route guards (`permissionGuard`) and template directives (`*ngxPermissionsOnly`), and `@accionadev/ui-kit`.

---

## Default Seeded Credentials (Development Only)

| Role | Email | Password | Permissions |
| :--- | :--- | :--- | :--- |
| `Admin` | `admin@enterprise.local` | `Admin123*` | All `Users.*`, `Roles.*`, `Categories.*`, `Products.*`, `InventoryMovements.*` |
| `User` | `user@enterprise.local` | `User123*` | `Categories.Read`, `Products.Read`, `InventoryMovements.Read` |

---

## Declaración de Uso de IA, Orquestación y Validación (Requisito Redarbor)

De acuerdo con los lineamientos de la prueba técnica, a continuación se declaran explícitamente las herramientas de Inteligencia Artificial utilizadas, las fases en las que intervinieron y el proceso de revisión/validación aplicado sobre su salida:

### 1. Herramientas de IA y Orquestación Utilizadas
- **Google Antigravity IDE (Arquitectura Plan / Execute en dos fases):** Orquestador principal de planificación arquitectónica, generación de código y verificación automatizada.
- **Sistema de Gobernanza, Reglas y Skills del Repositorio:**
  - Manual Maestro [`AGENTS.md`](AGENTS.md), perfil sellado [`PROJECT_PROFILE.md`](PROJECT_PROFILE.md), constitución arquitectónica [`.agents/CONSTITUTION.md`](.agents/CONSTITUTION.md) y reglas de Clean Code en [`.agents/rules/redarbor-clean-code.md`](.agents/rules/redarbor-clean-code.md).
  - Catálogo modular de **23 skills** especializadas (`.agents/skills/`), destacando `project-init`, `be-dotnet-scaffold`, `be-dotnet-test`, `fe-angular-scaffold`, `fe-angular-test`, `be-database-sp` y `ops-github-gitflow`.
- **Graphify AST Knowledge Graph (`graphify-out/`):** Indexación AST local de código fuente para recuperación de dependencias a nivel de símbolo y reducción de consumo de tokens.

### 2. Fases de Aplicación de la IA

| Fase | Qué generó / asistió la IA | Estándares aplicados |
| :--- | :--- | :--- |
| **1. Gobernanza y Reglas de Calidad** | Sellado de decisiones arquitectónicas en `PROJECT_PROFILE.md` y creación de `.agents/rules/redarbor-clean-code.md`. | Restricción de 7 reglas Clean Code de Redarbor, separación EF Core/Dapper y cero `FOREIGN KEY`. |
| **2. Infraestructura y Contenedores** | Generación de `docker-compose.yml` (SQL Server 2022 + Redis 7), `src/backend/Dockerfile` y plantilla `.env.example`. | HealthChecks, aislamiento de credenciales en `.env` (excluido en `.gitignore`) y carga desde variables de entorno. |
| **3. Backend (.NET 10 Onion + CQRS)** | Scaffolding de los 9 proyectos en `src/backend/EnterpriseApp.sln`, seguridad OAuth2/JWT + PBAC (`[HasPermission]`, rotación SHA-256 de Refresh Tokens y lista negra JTI O(1)) y migración inicial. | 1 tipo público por archivo `.cs`, `ErrorOr<T>`, `ProblemDetails` RFC 7807, lecturas EF Core `.AsNoTracking()` (`IFinder`) y escrituras Dapper (`IRepository`). |
| **4. Frontend (Angular 22+ Zoneless)** | Scaffolding en `src/frontend/` conectado a `@accionadev/ui-kit`, Signals, hidratación de sesión vía `provideAppInitializer` (`GET /api/v1/auth/me`), `tokenInterceptor` (Silent Refresh), `permissionGuard` y `*ngxPermissionsOnly`. | Cero ZoneJS, componentes reutilizados de `@accionadev/ui-kit` y Tailwind CSS con tokens semánticos. |
| **5. Pruebas, CI/CD y Optimización de Tokens** | Pruebas unitarias/integración (xUnit, Moq, FluentAssertions, `WebApplicationFactory`, Jest Zoneless), workflow `.github/workflows/ci.yml` y generación del grafo AST en `graphify-out/`. | Ejecución focalizada de pruebas en TDD, consultas AST con `graphify` y política de no hacer auto-commits sin validación. |

### 3. Cómo se Revisó y Validó la Salida de la IA
1. **Auditoría contra las 7 Reglas de Clean Code de Redarbor:**
   - Código fuente y comentarios 100% en inglés (mensajes UI orientados al usuario en español).
   - Cero líneas en blanco dentro de métodos/funciones salvo una línea previa al `return`.
   - Nombres de métodos y variables autodescriptivos.
   - Métodos cortos (`<= 25` líneas) y baja complejidad (`<= 1` nivel de anidación).
   - Configuración leída desde variables de entorno (`.env` / `EnvironmentLoader` / `IConfiguration`).
   - Máximo una `class` o `enum` por archivo.
   - Paso de objetos/DTOs de comando/consulta en lugar de múltiples propiedades sueltas.
2. **Validación de Esquema SQL Server sin Foreign Keys:**
   - Inspección directa de configuraciones EF Core y de la migración `20261115031703_InitialAuthMigration.cs` verificando **0** restricciones `ForeignKey`, ejecutada sobre el contenedor `redarbor-sqlserver` (`dotnet ef database update`).
3. **Verificación de Infraestructura, Compilación y Suites de Pruebas:**
   - `docker compose ps`: servicios `redarbor-sqlserver` (`1433`) y `redarbor-redis` (`6379`) en estado `Up (healthy)`.
   - `dotnet build src/backend/EnterpriseApp.sln --configuration Release`: `0` advertencias y `0` errores.
   - `dotnet test src/backend/EnterpriseApp.sln --configuration Release`: `19/19` pruebas unitarias (`Tests.UnitTests`) y `5/5` pruebas de integración (`Tests.IntegrationTests`, en memoria y contra `redarbor-sqlserver` en Docker) aprobadas.
   - `npm run test:ci` (en `src/frontend/`): `5/5` suites y `10/10` pruebas Jest Zoneless aprobadas.
   - `npm run build` (en `src/frontend/`): compilación de producción completada sin errores en `src/frontend/dist/frontend`.
