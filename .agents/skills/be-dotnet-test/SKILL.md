---
name: be-dotnet-test
description: Senior QA Automation and Backend Test Engineer skill for writing unit and integration tests in .NET 10. Use when generating mirror-structured unit tests (Domain, Handlers, Validators, Middlewares, Controllers) and end-to-end WebApplicationFactory integration tests under TDD.
---


# Skill: .NET Test Automation (xUnit + FluentAssertions + Moq + WebApplicationFactory)

## Rol y Capacidades

Actúas como un **Senior QA Automation & Backend Test Engineer** especializado en **.NET 10** y **C# 13**. Tu objetivo es diseñar suites de **Pruebas Unitarias** (estructura espejo aislada con `Moq`) y **Pruebas de Integración** (`WebApplicationFactory<Program>` validando el pipeline HTTP completo, middlewares de seguridad, PBAC y persistencia).

---


## 🏛️ Pirámide Completa de Pruebas Backend


### A. Pruebas Unitarias (`tests/Tests.UnitTests` — Aisladas con Moq)

1. **Dominio (`Core/Domain/`):** Invariantes de agregados/entidades (`User`, `RefreshToken`), Value Objects y eventos de dominio.
2. **Handlers CQRS (`Core/Application/Features/...`):** Casos de éxito y fallo tipados con `ErrorOr<T>`, verificando `IUnitOfWork.SaveChangesAsync` (`Times.Once` vs `Times.Never`).
3. **Validadores (`FluentValidation`):** Uso de `TestValidate()` y `ShouldHaveValidationErrorFor`.
4. **Middlewares y Controladores (`Presentation/`):** Validación directa de `JwtBlacklistMiddleware` (`401`), `PermissionAuthorizationMiddleware` (`403`) y mapeo RFC 7807 `ProblemDetails` en controladores.


### B. Pruebas de Integración (`tests/Tests.IntegrationTests` — Pipeline Real HTTP)

1. **`CustomWebApplicationFactory : WebApplicationFactory<Program>`:**
   - Levanta el host real de `Web.API` en memoria (`Environment = "Testing"`).
   - Soporta PostgreSQL real mediante variable `TEST_POSTGRES_CONNECTION` o base aislada en memoria eliminando descriptores previos (`DbContextOptions<AppDbContext>` e `IDbContextOptionsConfiguration`) en EF Core 10.
   - Ejecuta `InitialDataSeeder.SeedAsync` automáticamente al iniciar el servidor de pruebas.
2. **Cobertura End-to-End Mandatoria por Feature:**
   - Salud del servicio (`GET /health` ➔ `200 OK`).
   - Ciclo de sesión (`POST /auth/login` ➔ `GET /auth/me` con permisos ➔ `POST /auth/refresh` rotación RTR + detección de reúso `401` ➔ `POST /auth/logout` Blacklist `401`).
   - Seguridad PBAC de Endpoints: `200 OK` / `201 Created` cuando el rol posee el permiso requerido en `[HasPermission]`, y `403 Forbidden` (RFC 7807) cuando carece de él.

---

## 📂 Estructura Física Espejo Mandatoria (`tests/`)

  Queda prohibido arrojar archivos de prueba sueltos en la raíz del proyecto de tests. Deben reflejar exactamente las capas de `src/`:

```text
tests/
├── Tests.UnitTests/
│   ├── Core/
│   │   ├── Domain/
│   │   │   └── Entities/
│   │   │       ├── UserTests.cs
│   │   │       └── RefreshTokenTests.cs
│   │   └── Application/
│   │       └── Features/<Feature>/
│   │           ├── Commands/<Action>/
│   │           │   ├── <Action>CommandHandlerTests.cs
│   │           │   └── <Action>CommandValidatorTests.cs
│   │           └── Queries/<Action>/
│   │               └── <Action>QueryHandlerTests.cs
│   └── Presentation/
│       ├── Middlewares/
│       │   ├── JwtBlacklistMiddlewareTests.cs
│       │   └── PermissionAuthorizationMiddlewareTests.cs
│       └── Controllers/
│           └── <Feature>ControllerTests.cs
│
└── Tests.IntegrationTests/
    ├── Common/
    │   ├── CustomWebApplicationFactory.cs
    │   └── IntegrationTestBase.cs
    └── Features/
        ├── Auth/
        │   └── AuthFlowIntegrationTests.cs
        └── <Feature>/
            └── <Feature>EndpointsIntegrationTests.cs
```

---

## 📁 Catálogo de Ejemplos (`examples/`)

- `handler-unit-test.example.cs`: Prueba unitaria de CommandHandler con xUnit, Moq y FluentAssertions.
- `validator-unit-test.example.cs`: Prueba de FluentValidation con `TestValidate()`.
- `security-middleware-test.example.cs`: Prueba unitaria de middleware PBAC contra caché.
- `integration-endpoint-test.example.cs`: Configuración completa de `CustomWebApplicationFactory`, `IntegrationTestBase` y pruebas HTTP de extremo a extremo (`200`, `401`, `403`).

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Estructura Espejo Respetada** — Ningún test suelto en la raíz; carpetas alineadas con `Core/`, `Presentation/` y `Features/`.
- [ ] **Estructura AAA Explícita** — Bloques `Arrange`, `Act` y `Assert` claramente delimitados.
- [ ] **Aislamiento en UnitTests vs Pipeline Real en IntegrationTests** — UnitTests 100% con `Moq`; IntegrationTests con `HttpClient` sobre `CustomWebApplicationFactory`.
- [ ] **Verificación PBAC HTTP Real** — Cada nuevo endpoint protegido con `[HasPermission]` valida tanto respuesta autorizada como rechazo `403 Forbidden`.
- [ ] **Nomenclatura Estandarizada** — `Should_ExpectedBehavior_When_Condition`.
- [ ] **Filtro Focalizado en TDD** — `dotnet test --filter FullyQualifiedName~<FeatureName>` durante ciclos Red/Green.
