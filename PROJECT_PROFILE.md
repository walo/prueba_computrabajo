# Project Profile: Redarbor Product Inventory Management System

> **Active Architecture Profile (Sealed in `/project-init`)**

## 1. Tenancy & Security Model
- **Tenancy:**
  - [x] **Single-Tenant / User-Centric** (No `TenantId`, no organization tables, no multi-tenant RLS)
  - [ ] Multi-Tenant / B2B
- **Security Level:**
  - [x] **Level 3 PBAC (Permission-Based Access Control)** — OAuth2/JWT ultralight Bearer tokens, Refresh Token Rotation (RTR) with SHA-256 hashing & intrusion detection, O(1) JTI blacklist, `[HasPermission(...)]` on backend endpoints, and `*ngxPermissionsOnly` + `permissionGuard` on frontend.

## 2. Target Platforms
- [x] **Backend .NET 10** — Decoupled Onion Architecture (`Core/Domain`, `Core/Application`, `Infrastructure/{Authentication,ExternalServices,Persistence,Shared}`, `Presentation/Web.API`, `Tests/{Tests.UnitTests,Tests.IntegrationTests}`).
- [x] **Web Frontend (Angular 22+ Zoneless)** — Standalone components, `ChangeDetectionStrategy.OnPush`, Signals, Jest (`jest-preset-angular`), Tailwind CSS v3, connected to `@accionadev/ui-kit` (`../ui-design-system`).
- [ ] **App Mobile** — Not required.

## 3. Database & CQRS Persistence Strategy
- **Engine:** SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`) + Redis 7 (`redis:7-alpine`) via Docker Compose.
- **CQRS Persistence Split:**
  - **Read Operations (`Queries` / `IFinder`):** Entity Framework Core with `.AsNoTracking()` projections.
  - **Write Operations (`Commands` / `IRepository`):** **Dapper** (`IDbConnectionFactory` / `SqlConnection`) executing parameterized SQL statements (with EF Core InMemory fallback when running isolated integration tests on non-relational test providers).
- **Schema Constraint (Zero Foreign Keys):**
  - **No `FOREIGN KEY` constraints** are permitted in SQL Server tables, EF Core configurations, or migrations. Relationships are modeled via scalar IDs (`RoleId`, `UserId`, `PermissionId`, `CategoryId`, `ProductId`) with non-FK indexes.

## 4. Redarbor Clean Code Rules (Mandatory)
1. **100% English:** All code, identifiers, types, tests, and comments in English (only user-facing localized error/UI response messages may use Spanish if required).
2. **Blank Lines Rule:** Zero blank lines inside method/function bodies, except a single blank line immediately before a `return` statement.
3. **Self-Descriptive Naming:** Method and function names must clearly express intent without requiring explanatory comments.
4. **Single Responsibility Functions:**
   - Maximum **25 lines** per method/function.
   - Maximum **1 level** of nested loops or conditionals.
   - Zero sections inside functions that require explanatory comments.
5. **Environment Configuration:** All configuration values must be read from environment variables / `IConfiguration`.
6. **One Type Per File:** Strictly 1 public `class`, `enum`, `interface`, or `record` per physical file (`.cs` / `.ts`).
7. **Object Parameters:** Pass cohesive objects/records instead of multiple primitive properties as method parameters wherever possible (e.g., `Product.Create(ProductCreationData data)`).
