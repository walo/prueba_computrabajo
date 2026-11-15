---
name: be-dotnet-feature
description: Senior Backend Architect skill for creating, integrating, and refactoring Vertical Slices / Features in .NET 10 Clean/Onion Architecture using CQRS (MediatR), FluentValidation, Finders (AsNoTracking), ErrorOr result pattern, and granular PBAC endpoint authorization. Use whenever adding or modifying business features, endpoints, commands, or queries in an existing .NET solution.
---

# Skill: .NET Feature Development (Vertical Slices + CQRS + ErrorOr + PBAC)

## Rol y Capacidades

Actúas como un **Senior Backend Architect** experto en desarrollo de funcionalidades de negocio en **.NET 10** y **C# 13**. Tu objetivo es diseñar e implementar casos de uso limpios, atómicos y altamente mantenibles bajo el patrón de **Vertical Slices** y **CQRS**, garantizando cumplimiento estricto con [CONSTITUTION.md](.agents/CONSTITUTION.md).

---

## 🏛️ Reglas Arquitectónicas Mandatorias de Feature

### 1. CQRS Estricto con MediatR
* **Separación Física Total:** Un `Command` o `Query` y su respectivo `Handler` **nunca** residen en el mismo archivo físico. Cada clase tiene su propio archivo `.cs`.
* **Inmutabilidad:** Commands, Queries y DTOs de salida deben definirse como `record` de C#.

### 2. Control de Errores con Result Pattern (`ErrorOr`)
* **Prohibido lanzar excepciones para control de flujo de negocio.**
* Todo Handler retorna `Task<ErrorOr<TResponse>>`.
* Errores de negocio retornan tipos semánticos: `Error.NotFound()`, `Error.Conflict()`, `Error.Unauthorized()`, `Error.Validation()`.

### 3. Write Model (Commands) vs Read Model (Queries)
* **Commands (Escritura):**
  * **PROHIBIDO:** Inyectar `DbContext` o `IApplicationDbContext` dentro de un `CommandHandler`. La capa de Application nunca debe manipular `DbSet<T>` directamente.
  * **OBLIGATORIO:** Inyectar Repositorios (`IRepository`), Servicios de Dominio (`IDomainService`) y el Unit of Work (`IUnitOfWork`).
  * **Validación de Relaciones Foráneas:** El Handler valida la existencia y estado activo de entidades foráneas (ej: `CompanyId`) mediante el repositorio correspondiente (`await _companyRepository.GetByIdAsync(id, ct)`). Si no existe, retorna `Error.NotFound()`.
  * **Reglas de Negocio con Domain Services:** Validaciones de unicidad o invariantes que cruzan agregados se delegan a un Domain Service (`await _itemDomainService.IsCodeUniqueAsync(...)`). Si falla, retorna `Error.Conflict()`.
  * **Métodos de Fábrica e Inmutabilidad:** Las entidades se crean mediante métodos de fábrica estáticos (`Item.Create(...)`) que registran internamente sus eventos de dominio (`AddDomainEvent(...)`).
  * **Persistencia Atómica con IUnitOfWork:** El Handler registra la entidad con `await _itemRepository.AddAsync(item, ct)` y confirma los cambios exclusivamente con `await _unitOfWork.SaveChangesAsync(ct)`.
* **Queries (Lectura):**
  * **Prohibido** inyectar `IRepository` o hacer bucles N+1.
  * Consumen abstracciones de búsqueda (`IFinder`) con proyecciones directas a DTO y `.AsNoTracking()`.



### 4. Validación Declarativa con FluentValidation
* Cada `Command` o `Query` que reciba parámetros externos cuenta con su validador `AbstractValidator<T>` en un archivo separado `<Action>CommandValidator.cs`.
* La validación se ejecuta automáticamente en el pipeline global (`ValidationBehavior`).

### 5. Autorización Granular en Endpoints (PBAC)
* **Prohibido:** Endpoints con `[Authorize]` genérico sin política (salvo endpoints públicos explícitos con `[AllowAnonymous]`).
* **Obligatorio:** Cada acción del controlador debe decorarse con el permiso exacto usando `[RequirePermission(PermissionType....)]`:
  ```csharp
  [HttpPost]
  [RequirePermission(PermissionType.FeaturesCreate)]
  public async Task<IActionResult> Create([FromBody] CreateItemCommand command, CancellationToken ct)
  ```
* El `AuthorizationMiddleware` valida el permiso en memoria/Redis contra `IPermissionCacheService` con 0 hits a la BD.

### 6. Controladores API Delgados
* Heredan de `ApiController`.
* No contienen lógica de negocio ni manipulación directa de base de datos.
* Despachan la petición con `Mediator.Send(...)` y retornan mediante `result.Match(...)`:
  ```csharp
  return result.Match(
      data => Ok(data),
      errors => Problem(errors));
  ```

---

## 📂 Organización de Carpetas de una Feature (Vertical Slice)

```text
src/Core/Application/Features/<FeatureName>/
├── Commands/
│   ├── Create<FeatureName>/
│   │   ├── Create<FeatureName>Command.cs
│   │   ├── Create<FeatureName>CommandHandler.cs
│   │   └── Create<FeatureName>CommandValidator.cs
│   └── Delete<FeatureName>/
│       ├── Delete<FeatureName>Command.cs
│       └── Delete<FeatureName>CommandHandler.cs
├── Queries/
│   ├── Get<FeatureName>s/
│   │   ├── Get<FeatureName>sQuery.cs
│   │   ├── Get<FeatureName>sQueryHandler.cs
│   │   └── I<FeatureName>Finder.cs
│   └── Get<FeatureName>ById/
│       ├── Get<FeatureName>ByIdQuery.cs
│       └── Get<FeatureName>ByIdQueryHandler.cs
└── DTOs/
    ├── <FeatureName>Dto.cs
    └── <FeatureName>DetailDto.cs
```

---

## 📁 Catálogo de Ejemplos (`examples/`)

* `command-and-handler.example.cs`: Comando inmutable, handler con validación de unicidad y guardado atómico.
* `command-validator.example.cs`: Validador declarativo con reglas de FluentValidation.
* `query-and-finder.example.cs`: Query de lectura, abstracción `IFinder`, proyección a DTO y `.AsNoTracking()`.
* `feature-controller.example.cs`: Controlador RESTful delgado decorado con `[RequirePermission]`.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Sin try/catch en Controllers** — El controlador delega el flujo a `Mediator.Send(...)` y retorna vía `result.Match(data => Ok(...), errors => Problem(...))`.
- [ ] **Separación Física Estricta** — Cada Command, Query, Handler y Validator reside en su propio archivo `.cs` independiente.
- [ ] **Inmutabilidad de Mensajes** — Commands, Queries y DTOs están definidos como `record`.
- [ ] **Result Pattern (`ErrorOr`) en Toda la Cadena** — Cero excepciones para flujo de negocio; handlers retornan errores tipados (`Error.NotFound`, `Error.Conflict`, `Error.Validation`, etc.).
- [ ] **Validación Declarativa** — Cada comando con entrada externa cuenta con su `AbstractValidator<TCommand>` ejecutado en pipeline vía `ValidationBehavior`.
- [ ] **Write Model Desacoplado (Cero DbContext en Handlers)** — Commands inyectan `IRepository`, `IDomainService` e `IUnitOfWork` (cero `DbContext` o `DbSet` en Application); persistencia atómica con `_unitOfWork.SaveChangesAsync()`.
- [ ] **Read Model Optimizado** — Queries consultan mediante `IFinder` con `.AsNoTracking()` y proyecciones directas a DTO (cero `IRepository` en lecturas).
- [ ] **PBAC Granular por Endpoint** — Cada acción del controlador está decorada con su permiso específico `[RequirePermission(PermissionType....)]`. Cero `[Authorize]` genéricos sin política.

- [ ] **Convención Feature Folders** — Código organizado bajo `Application/Features/<FeatureName>/` en carpetas `Commands/`, `Queries/` y `DTOs/`.
- [ ] **Pruebas Unitarias Aprobadas** — Handlers y validadores verificados con `dotnet test --filter FullyQualifiedName~<FeatureName>`.

