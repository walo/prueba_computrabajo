# Entrevista Técnica y de Negocio: Consulta de Productos por Categoría

**Historia ID / GitHub Issue**: US-004  
**Issue URL**: https://github.com/walo/prueba_computrabajo/issues/8  
**Fecha de la Sesión**: 2026-11-20  
**Entrevistador**: Agente IA (Antigravity)  
**Entrevistado (Stakeholder/PO/Dev)**: Walter Urrego (Technical Lead)  
**Estado Issue**: In Progress (Rama `feature/US-004-consulta_de_productos_por_categoria`)  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada y gestionada en GitHub Issue #8  

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
La funcionalidad **Consulta de Productos por Categoría** complementa el catálogo de categorías (`US-001`) y el catálogo maestro de productos (`US-002`) incorporando un endpoint RESTful dedicado de solo lectura para consultar los productos pertenecientes a una categoría puntual a partir de su `categoryId`. A diferencia del listado general `GET /api/v1/products`, este endpoint verifica obligatoriamente que la categoría exista en `dbo.Categories` antes de consultar sus productos asociados, distinguiendo inequívocamente entre una categoría inexistente (`404 Not Found`) y una categoría válida sin productos (`200 OK` con lista vacía).

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - Nuevo endpoint `GET /api/v1/categories/{categoryId:guid}/products` en `CategoriesController` protegido con `[HasPermission(Permissions.ProductsRead)]`.
  - Nuevos ValueObjects / Parameter Objects en `Core/Domain/ValueObjects/`: `CategoryProductsFilter` (`Page`, `PageSize`, `Search`, `IsActive`, `LowStockOnly`, sin exponer `CategoryId` redundante en query string) y `CategoryProductsQuery` (`CategoryId`, `Filter`), cumpliendo la Regla 7 de Redarbor.
  - Nuevo Vertical Slice CQRS en `Core/Application/Features/Products/Queries/GetProductsByCategory/`: `GetProductsByCategoryQuery`, `GetProductsByCategoryQueryValidator` (FluentValidation) y `GetProductsByCategoryQueryHandler` (ErrorOr).
  - Extensión de `ICategoryFinder` / `CategoryFinder` con `ExistsAsync(Guid id, CancellationToken ct)` mediante EF Core `.AsNoTracking().AnyAsync(...)` y reutilización directa de `IProductFinder.GetPagedAsync(ProductFilter, ct)`.
  - Soporte para categorías *Activas* e *Inactivas* (preservando trazabilidad histórica) y respuesta `200 OK` con `Items = []` y `TotalCount = 0` si una categoría existente no posee productos que coincidan.
  - Pruebas unitarias (`Tests.UnitTests`) e integración E2E (`Tests.IntegrationTests`) bajo TDD y ejemplo de invocación en `src/backend/src/Presentation/Web.API/Web.API.http`.
  - Cumplimiento estricto de las **7 Reglas de Clean Code Redarbor** (`.agents/rules/redarbor-clean-code.md`).
- **Fuera del alcance explícito (Out-of-Scope)**:
  - Cambios en el frontend web Angular (`src/frontend/`), nuevas pantallas, rutas o modificaciones de servicios Angular en esta iteración.
  - Migraciones de base de datos o alteraciones de esquema (`dbo.Categories` y `dbo.Products` con el índice `IX_Products_CategoryId` ya existen sin `FOREIGN KEY`).
  - Aplicaciones móviles (`App Mobile` deshabilitada en `PROJECT_PROFILE.md`).
  - Aislamiento multi-tenant por `tenant_id` o políticas RLS organizacionales (`Single-Tenant` en `PROJECT_PROFILE.md`).

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

### Bloque A: Dominio, Contrato de Entrada y API RESTful

#### Pregunta A.1: Ruta RESTful y Controlador propietario del endpoint
- **Contexto y Desafío Identificado**: `US-002` ya ofrece filtrado por `categoryId` en `GET /api/v1/products`, por lo que se requiere ubicar el endpoint dedicado respetando la semántica RESTful y la estructura modular existente en `Presentation/Web.API/Controllers/`.
- **Opciones Evaluadas**:
  - *Opción 1*: `GET /api/v1/categories/{categoryId:guid}/products` en `CategoriesController` (sub-recurso RESTful jerárquico, decorado con `[HasPermission(Permissions.ProductsRead)]`).
  - *Opción 2*: `GET /api/v1/products/by-category/{categoryId:guid}` en `ProductsController`.
  - *Opción 3*: Exponer ambos endpoints como alias simultáneos.
- **Decisión Acordada**: Opción 1 (`GET /api/v1/categories/{categoryId:guid}/products` en `CategoriesController`).
- **Justificación Técnica & Regla Resultante**: Modela la relación jerárquica `Categoría -> Productos` de forma estándar RESTful sin duplicar rutas ni fragmentar la documentación Swagger.

#### Pregunta A.2: Contrato de Query Parameters y Regla 7 de Redarbor (Objetos como parámetros)
- **Contexto y Desafío Identificado**: Reutilizar directamente `ProductFilter` en `[FromQuery]` expondría un parámetro `categoryId` redundante en la query string y en Swagger al lado del `{categoryId:guid}` de la ruta.
- **Opciones Evaluadas**:
  - *Opción 1*: Crear el ValueObject dedicado `CategoryProductsFilter` (`Page = 1`, `PageSize = 10`, `Search = null`, `IsActive = null`, `LowStockOnly = null`) junto con `CategoryProductsQuery(Guid CategoryId, CategoryProductsFilter Filter)` y `GetProductsByCategoryQuery(CategoryProductsQuery Request)`.
  - *Opción 2*: Reutilizar `ProductFilter` directamente en `[FromQuery]` sobrescribiendo `CategoryId`.
- **Decisión Acordada**: Opción 1 (`CategoryProductsFilter` + `CategoryProductsQuery` + `GetProductsByCategoryQuery`).
- **Justificación Técnica & Regla Resultante**: Elimina ambigüedades en el contrato OpenAPI/Swagger, mantiene la separación de un tipo público por archivo (Regla 6 de Redarbor) y pasa objetos cohesivos en lugar de propiedades primitivas sueltas (Regla 7 de Redarbor). El handler proyecta `CategoryProductsQuery` hacia `ProductFilter` forzando `CategoryId = request.CategoryId`.

#### Pregunta A.3: Regla ante categoría inexistente vs. categoría inactiva y código de error RFC 7807
- **Contexto y Desafío Identificado**: Se requiere diferenciar claramente una categoría inexistente de una categoría válida sin productos, así como definir el comportamiento para categorías en estado *Inactivo* y el código de error RFC 7807 exacto.
- **Opciones Evaluadas**:
  - *Opción 1*: Validar existencia vía `ICategoryFinder.ExistsAsync(categoryId, ct)`. Si no existe, retornar `Error.NotFound("Category.NotFound", "La categoría solicitada no existe.")` (`404 Not Found` RFC 7807). Si existe (esté *Activa* o *Inactiva*), retornar `200 OK` con `PagedProductsResponse` (lista vacía y `TotalCount = 0` si no hay coincidencias).
  - *Opción 2*: Usar `Product.CategoryNotFound` o rechazar también categorías inactivas.
- **Decisión Acordada**: Opción 1 (`Category.NotFound` para inexistencia y consulta permitida sobre categorías inactivas).
- **Justificación Técnica & Regla Resultante**: Reutiliza el mismo código `Category.NotFound` de `GetCategoryByIdQueryHandler` (ya registrado en `error-messages.json`), evita romper trazabilidad histórica de productos vinculados a categorías inactivas y garantiza respuesta consistente cuando una categoría válida aún no tiene productos asociados.

---

### Bloque B: Roles, Permisos y Seguridad (PBAC)

#### Pregunta B.1: Permisos PBAC y alcance Single-Tenant aplicables al endpoint
- **Contexto y Desafío Identificado**: Aunque la ruta reside bajo `/api/v1/categories/{categoryId:guid}/products`, el recurso consultado y retornado es el catálogo de productos (`PagedProductsResponse`).
- **Decisión Acordada**:
  - Permiso obligatorio en el controlador: `[HasPermission(Permissions.ProductsRead)]` (`"Products.Read"`), ya definido en `Domain/Constants/Permissions.cs` y asignado en `InitialDataSeeder.cs` tanto al rol `ADMIN` como al rol de solo lectura `USER`.
  - Respuestas de seguridad garantizadas por los middlewares existentes (`JwtBlacklistMiddleware` y `PermissionAuthorizationMiddleware`): `401 Unauthorized` si falta token o está revocado, y `403 Forbidden` (RFC 7807) si el usuario autenticado carece de `Products.Read`.
- **Alcance de Datos (Single-Tenant)**:
  - Conforme a `PROJECT_PROFILE.md`, el sistema opera en modo **Single-Tenant / User-Centric** con Nivel 3 PBAC. No aplica aislamiento por tenant_id ni RLS multi-tenant en las consultas de categorías o productos.

---

### Bloque C: Experiencia de Usuario, Persistencia y Alcance Frontend

#### Pregunta C.1: Alcance de Frontend Angular y estrategia de lectura en SQL Server
- **Contexto y Desafío Identificado**: Confirmar si se requiere algún cambio en `src/frontend/` (servicio HTTP o componente visual) y validar si `dbo.Products` requiere cambios de esquema.
- **Opciones Evaluadas**:
  - *Opción 1*: Alcance 100% Backend (`.NET 10` CQRS + EF Core `.AsNoTracking()` + PBAC + Unit/Integration Tests + ejemplo en `Web.API.http`); cero cambios en `src/frontend/`.
  - *Opción 2*: Agregar también método en `ProductsService` de Angular.
- **Decisión Acordada**: Opción 1 (100% Backend; cero cambios en `src/frontend/`).
- **Justificación Técnica & Regla Resultante**: Cumple el requerimiento estricto de `US-004` sin introducir código muerto ni UI no solicitada. En persistencia, `CategoryFinder.ExistsAsync` ejecuta `.AsNoTracking().AnyAsync(...)` sobre `dbContext.Categories` y `ProductFinder.GetPagedAsync` aprovecha el índice no único existente `IX_Products_CategoryId` sin crear migraciones ni restricciones `FOREIGN KEY`.

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (SQL Server 2022)** | Sin nueva migración; uso del índice no único existente `IX_Products_CategoryId` y `0` restricciones `FOREIGN KEY` | Esquema de `Categories` y `Products` ya desplegado en `US-001` y `US-002`; cero impacto destructivo |
| **Core / Domain (.NET 10)** | Nuevos ValueObjects `CategoryProductsFilter` y `CategoryProductsQuery` (`1` tipo por archivo); reutilización de `Permissions.ProductsRead` | Reglas 6 y 7 de Redarbor Clean Code; evita exponer `CategoryId` duplicado en query string |
| **Core / Application (.NET 10)** | Slice `GetProductsByCategory` (Query, Validator FluentValidation, Handler `ErrorOr`) + método `ICategoryFinder.ExistsAsync` | Onion Architecture estricta (solo `IFinder`, cero `DbContext` o `IRepository` en Query Handler) |
| **Infrastructure / Persistence** | Implementar `CategoryFinder.ExistsAsync` con EF Core `.AsNoTracking().AnyAsync(...)` y reutilizar `ProductFinder.GetPagedAsync` | Respeto total al split CQRS (EF Core `.AsNoTracking()` para lecturas, Dapper reservado a escrituras) |
| **Presentation / Web.API** | Acción `GetProductsByCategory` en `CategoriesController` (`GET /api/v1/categories/{categoryId:guid}/products`) con `[HasPermission(Permissions.ProductsRead)]` + ejemplo en `Web.API.http` | Semántica RESTful jerárquica, seguridad PBAC granular y documentación Swagger/RFC 7807 |
| **Testing (TDD)** | Unit Tests (`GetProductsByCategoryQueryValidatorTests`, `GetProductsByCategoryQueryHandlerTests`) + Integration Tests (`CategoryProductsIntegrationTests`) | Verificación completa de validación (`400`), PBAC (`401`/`403`), existencia (`404`) y filtros/paginación (`200`) |
| **Frontend (Angular 22+)** | Fuera de alcance en `US-004` (`0` cambios en `src/frontend/`) | Acordado en entrevista; `Category.NotFound` ya existe en `error-messages.json` |
| **App Mobile** | N/A | Deshabilitado en `PROJECT_PROFILE.md` |

---

## 4. Criterios de Aceptación Refinados (Gherkin)

> Estos escenarios consolidan los criterios de aceptación del GitHub Issue #8 con los contratos técnicos acordados.

### Escenario 1 — Consulta exitosa de productos de una categoría existente con paginación y filtros (Camino Feliz)
- **Dado** un usuario autenticado con permiso `Products.Read` y una categoría activa existente (`categoryId`) con varios productos asociados en `dbo.Products`
- **Cuando** invoca `GET /api/v1/categories/{categoryId}/products?page=1&pageSize=10&search=LAP&isActive=true&lowStockOnly=true`
- **Entonces** `GetProductsByCategoryQueryHandler` confirma la existencia de la categoría vía `ICategoryFinder.ExistsAsync`, delega la lectura filtrada a `IProductFinder.GetPagedAsync` con `CategoryId` forzado al valor de la ruta y retorna `200 OK` con `PagedProductsResponse`
- **Y** cada elemento incluye los metadatos del producto, `categoryCode`, `categoryName` y el indicador `isLowStock` (`stock <= minStock`)

### Escenario 2 — Consulta de categoría inactiva y categoría válida sin productos (Caso Alternativo)
- **Dado** un usuario autenticado con permiso `Products.Read`, una categoría *Inactiva* con productos históricos asociados y una categoría *Activa* sin productos asociados
- **Cuando** invoca `GET /api/v1/categories/{inactiveCategoryId}/products` y posteriormente `GET /api/v1/categories/{emptyCategoryId}/products`
- **Entonces** para la categoría *Inactiva* el sistema retorna `200 OK` con sus productos históricos paginados preservando la trazabilidad
- **Y** para la categoría sin productos el sistema retorna `200 OK` con `items: []` y `totalCount: 0` sin error

### Escenario 3 — Rechazo controlado cuando la categoría consultada no existe (Rechazo Controlado)
- **Dado** un usuario autenticado con permiso `Products.Read` y un `categoryId` válido en formato GUID que no existe en `dbo.Categories`
- **Cuando** invoca `GET /api/v1/categories/{nonExistentCategoryId}/products`
- **Entonces** el handler detecta `ExistsAsync == false` sin invocar `IProductFinder.GetPagedAsync` y responde `404 Not Found` bajo RFC 7807 (`ProblemDetails`)
- **Y** el título del error es `Category.NotFound` con el detalle `"La categoría solicitada no existe."`

### Escenario 4 — Acceso permitido a Operador de solo lectura y rechazo sin sesión o sin permiso (Seguridad PBAC)
- **Dado** una categoría existente, un usuario `USER` con permiso `Products.Read`, un usuario autenticado sin `Products.Read` y una petición anónima sin cabecera `Authorization`
- **Cuando** cada solicitante invoca `GET /api/v1/categories/{categoryId}/products`
- **Entonces** el sistema retorna `200 OK` al usuario `USER` con `Products.Read`
- **Y** retorna `401 Unauthorized` al solicitante sin sesión activa y `403 Forbidden` (RFC 7807) al usuario autenticado sin permiso `Products.Read`

### Escenario 5 — Rechazo ante parámetros de paginación o filtros inválidos (Límite Operativo)
- **Dado** un usuario autenticado con permiso `Products.Read`
- **Cuando** invoca `GET /api/v1/categories/{categoryId}/products` con `page=0`, `pageSize=101`, `search` superior a `150` caracteres o `categoryId` igual a `Guid.Empty`
- **Entonces** `GetProductsByCategoryQueryValidator` intercepta la petición antes de ejecutar el handler y retorna `400 Bad Request` con `ValidationProblemDetails` (RFC 7807)
- **Y** no se ejecutan consultas hacia la base de datos

---

## 5. Checklist de Preparación para SDD

- [x] Todas las preguntas técnicas y de negocio fueron respondidas y consensuadas con el usuario (`ask_question`).
- [x] El permiso requerido (`Permissions.ProductsRead`) ya existe en `Permissions.cs` y en `InitialDataSeeder.cs`.
- [x] El perfil `Single-Tenant` + `Level 3 PBAC` y la regla de `0` `FOREIGN KEY` de `PROJECT_PROFILE.md` están respetados explícitamente.
- [x] Las 7 Reglas de Clean Code Redarbor están incorporadas en el diseño de ValueObjects, Query, Validator y Handler.
- [x] El alcance `100% Backend` (sin cambios en `src/frontend/` ni `App Mobile`) está delimitado explícitamente.
- [x] El artefacto está guardado como `specs/US-004_consulta_de_productos_por_categoria/US-004-interview.md`.

> **Siguiente Paso**: Generar `US-004.spec.md`, `US-004-plan.md` y `US-004-tasks.md` en `specs/US-004_consulta_de_productos_por_categoria/`.
