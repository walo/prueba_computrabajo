# Entrevista Técnica y de Negocio: Catálogo de Categorías de Productos

**Historia ID / Jira Issue**: US-001 (GitHub Issue #1: `https://github.com/walo/prueba_computrabajo/issues/1`)  
**Fecha de la Sesión**: 2026-11-15  
**Entrevistador**: Antigravity Architect Agent  
**Entrevistado (Stakeholder/PO/Dev)**: Product Owner / Candidato Redarbor  
**Estado**: En Curso (In Progress)  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada en GitHub Issue #1 (`walo/prueba_computrabajo`)

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
El catálogo de **Categorías de Productos** es el primer módulo estructural del Sistema de Inventario de Redarbor. Permite organizar, clasificar y filtrar los productos de la empresa bajo códigos únicos y nombres estandarizados, asegurando la integridad referencial lógica mediante reglas de aplicación (sin llaves foráneas físicas `FOREIGN KEY` en SQL Server 2022). Operacionalmente, sirve de insumo directo para el posterior módulo de Productos e Inventario.

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - Entidad de dominio `Category` con `Code` (slug/código único), `Name` (obligatorio y único), `Description` (opcional) e `IsActive` (`true`/`false`), más timestamps UTC (`CreatedAtUtc`, `UpdatedAtUtc`).
  - API RESTful CRUD completo bajo `/api/v1/categories` con paginación en servidor (`Page`, `PageSize`), búsqueda (`Search`) y filtro por estado (`IsActive`).
  - Persistencia híbrida CQRS: lecturas con **Entity Framework Core `.AsNoTracking()`** (`ICategoryFinder`) y escrituras con **Dapper** (`ICategoryRepository` sobre `IDbConnectionFactory`).
  - Bloqueo controlado de eliminación si la categoría tiene productos asociados, sugiriendo inactivarla.
  - Paso obligatorio de **objetos como parámetro** (`CategoryCreationData`, `CategoryUpdateData`, `CategoryFilter`, `UpdateCategoryRequest`) cumpliendo las 7 reglas de Clean Code de Redarbor.
  - Interfaz Angular 22+ Zoneless conectada al shell lateral, protegida por `permissionGuard` y `*ngxPermissionsOnly`, utilizando exclusivamente componentes `<ad-*>` de `@accionadev/ui-kit`.
- **Fuera del alcance explícito (Out-of-Scope)**:
  - Jerarquías recursivas padre/hijo entre categorías o subcategorías anidadas.
  - Creación o edición de Productos e inventario (pertenece a historias posteriores del backlog).
  - Restricciones `FOREIGN KEY` o PostgreSQL RLS (proyecto **Single-Tenant** sobre **SQL Server 2022** según `PROJECT_PROFILE.md`).

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

### Bloque A: Dominio y Reglas de Negocio

#### Pregunta A.1: Atributos de identidad, unicidad y eliminación de Categorías
- **Contexto y Desafío Identificado**: La prueba técnica exige gestionar Categorías, Productos y Movimientos sin `FOREIGN KEY` físicas en base de datos, por lo que las invariantes de unicidad y protección al eliminar deben resolverse explícitamente en Dominio y Aplicación.
- **Opciones Evaluadas**:
  - *Opción 1*: Solo Nombre único + Descripción opcional. Menor trazabilidad para integraciones y filtros por código corto.
  - *Opción 2*: Código/Slug único + Nombre obligatorio y único + Descripción opcional + Estado `Activo/Inactivo`, bloqueando eliminación física cuando existan productos vinculados.
- **Decisión Acordada**: Opción 2 (Código/Slug único + Nombre obligatorio + Descripción opcional + Estado Activo/Inactivo, con bloqueo de eliminación si existen productos asociados).
- **Justificación Técnica & Regla Resultante**:
  - `Code`: obligatorio (`2–30` caracteres), normalizado en minúsculas y sin espacios extremos (`Trim().ToLowerInvariant()`), patrón slug/alfanumérico con guiones (`^[a-z0-9]+(?:-[a-z0-9]+)*$`), único globalmente.
  - `Name`: obligatorio (`2–100` caracteres, `Trim()`), único globalmente ignorando mayúsculas/minúsculas.
  - `Description`: opcional (hasta `500` caracteres).
  - `IsActive`: inicia en `true` al crearse; permite inactivación/reactivación mediante actualización de la categoría.

---

### Bloque B: Roles, Permisos y Seguridad (PBAC)

#### Pregunta B.1: Permisos granulares PBAC y modelo de aislamiento Single-Tenant
- **Contexto**: En `Domain/Constants/Permissions.cs` y `src/frontend/src/app/core/constants/permissions.constants.ts` ya existen definidos los permisos `Categories.Read`, `Categories.Create`, `Categories.Update` y `Categories.Delete`. Según `PROJECT_PROFILE.md`, el proyecto opera en modo **Single-Tenant** (sin `tenant_id` ni `company_id` por entidad ni RLS de PostgreSQL).
- **Decisión Acordada**:
  - Consulta paginada y detalle (`GET`): requiere `Categories.Read` (asignado a `Admin` y `User`).
  - Registro (`POST`): requiere `Categories.Create` (asignado a `Admin`).
  - Edición y activación/inactivación (`PUT`): requiere `Categories.Update` (asignado a `Admin`).
  - Eliminación (`DELETE`): requiere `Categories.Delete` (asignado a `Admin`).
- **Aislamiento y Tenancy**:
  - Aplicación Single-Tenant: los datos de `Categories` pertenecen al inventario global de la instancia, sin columna `tenant_id`/`company_id` ni políticas RLS; toda operación valida autenticación OAuth2/JWT Bearer + `JwtBlacklistMiddleware` + `PermissionAuthorizationMiddleware`.

---

### Bloque C: Experiencia de Usuario (UX) e Interacción Visual

#### Pregunta C.1: Flujo de pantalla, paginación y consumo exclusivo de `@accionadev/ui-kit`
- **Flujo de Pantalla Acordado**: Vista principal en ruta `/categories` dentro de `ShellComponent` con cabecera y botón `Nueva Categoría` (condicionado por `*ngxPermissionsOnly`), barra de filtros con buscador debounced y selector de estado (`Todos`, `Activos`, `Inactivos`), tabla paginada en servidor con acciones por fila (`Editar`, `Eliminar`), modal para crear/editar y diálogo de confirmación antes de eliminar.
- **Componentes `@accionadev/ui-kit` Existentes a Reutilizar** (verificados en `ui-manifest.json`):
  - `AdTableComponent` (`<ad-table>` con paginación `[totalItems]`, `[page]`, `[pageSize]`, `(pageChange)` y templates `ng-template[adTableCell]`).
  - `AdModalComponent` (`<ad-modal>` para formulario crear/editar).
  - `AdConfirmService` + `AdConfirmDialogComponent` (`<ad-confirm-dialog>` para confirmar eliminación).
  - `AdTableActionsComponent` (`<ad-table-actions>` para acciones de fila editar/eliminar).
  - `AdSearchBoxComponent` (`<ad-search-box>` con debounce `300ms`).
  - `AdSelectComponent` (`<ad-select>` para filtro de estado Activo/Inactivo/Todos).
  - `AdInputComponent` (`<ad-input>`), `AdTextareaComponent` (`<ad-textarea>`), `AdSwitchComponent` (`<ad-switch>`).
  - `AdButtonComponent` (`<ad-button>`), `AdBadgeComponent` (`<ad-badge>`), `AdCardComponent` (`<ad-card>`), `AdAlertComponent` (`<ad-alert>`).
- **Nuevos Componentes Requeridos en `@accionadev/ui-kit`**: Ninguno. El catálogo actual de `ui-manifest.json` cubre el 100% de los controles necesarios.
- **Estados Visuales Obligatorios**:
  - **Carga (Loading)**: Propiedad `[loading]` de `<ad-table>` con filas skeleton integradas y estado `[loading]` en botón de guardado del modal.
  - **Vacío (Empty State)**: `<ad-empty-state>` integrado en `<ad-table>` con título y descripción amigables (`emptyTitle` / `emptyDescription`).
  - **Error controlado**: Traducción semántica de errores HTTP RFC 7807 en alertas/toasts vía `NotificationService` y `mapHttpErrorToMessage`.
  - **Restricción de permisos**: Acceso directo a `/categories` sin `Categories.Read` redirige a `/forbidden` mediante `permissionGuard`; botones de creación/edición/eliminación ocultos con `*ngxPermissionsOnly`.

---

### Bloque D: Diseño Limpio (Paso de Objetos) y Casos de Borde

#### Pregunta D.1: Cumplimiento de la Regla 7 de Redarbor y manejo de errores RFC 7807
- **Escenario Límite / Falla**:
  1. Evitar firmas con múltiples propiedades escalares en métodos de Dominio, Repositorios, Servicios y Frontend (ej. nunca `Category.Update(code, name, desc, isActive)`).
  2. Duplicidad concurrente de `Code` o `Name` al crear o editar.
  3. Intento de eliminación de una categoría con productos vinculados en la tabla `Products`.
- **Comportamiento Esperado**:
  - Todas las operaciones reciben objetos de parámetro tipados: `CategoryCreationData`, `CategoryUpdateData`, `CategoryUniquenessCheck`, `CategoryFilter`, `UpdateCategoryRequest`.
  - Para cambiar el estado o editar campos de una categoría se utiliza `PUT /api/v1/categories/{id}` recibiendo un único DTO `UpdateCategoryRequest` como objeto completo, manteniendo cohesión y cumpliendo estrictamente la Regla 7 de Redarbor.
  - En SQL Server, la verificación de productos asociados inspecciona de manera segura la existencia de `dbo.Products` (`OBJECT_ID`) sin `FOREIGN KEY`, permitiendo operar de forma autónoma antes y después de implementar `Products`.
- **Manejo de Errores y Mensajería**:
  - `Category.NotFound` (`404 NotFound`)
  - `Category.DuplicateCode` (`409 Conflict`)
  - `Category.DuplicateName` (`409 Conflict`)
  - `Category.HasAssociatedProducts` (`409 Conflict` con mensaje sugiriendo inactivar la categoría)

---

### Bloque E: Persistencia, Integración y Migraciones

#### Pregunta E.1: Estrategia híbrida EF Core Lectura / Dapper Escritura en SQL Server 2022
- **Estrategia de Persistencia**: Nueva tabla `Categories` en SQL Server 2022 con índices únicos filtrados/estándar en `Code` y `Name` (colación case-insensitive o valores normalizados), sin restricciones `FOREIGN KEY`.
- **Lectura / Consulta**: `ICategoryFinder` en `Infrastructure/Persistence/Finders/CategoryFinder.cs` usando exclusivamente **Entity Framework Core `.AsNoTracking()`** para consultas paginadas, filtros y consulta por `Id`.
- **Escritura / Dominio**: `Category` (`AggregateRoot<Guid>`) en `Core/Domain/Entities/Category.cs` persistido por `ICategoryRepository` (`Infrastructure/Persistence/Repositories/CategoryRepository.cs`) ejecutando sentencias SQL parametrizadas con **Dapper** sobre `IDbConnectionFactory` (y respaldo InMemory cuando `!dbContext.Database.IsRelational()` para `WebApplicationFactory`).
- **Eventos de Dominio**: `CategoryCreatedDomainEvent`, `CategoryUpdatedDomainEvent`, `CategoryDeletedDomainEvent` definidos en `Core/Domain/Events/` y emitibles mediante `IPublisher`.

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (SQL Server 2022)** | Tabla `Categories` (`Id` PK sin identity, `Code` unique index, `Name` unique index, `Description`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`), cero `FOREIGN KEY` | Cumple restricción estricta de SQL Server sin FKs e índices para búsqueda y paginación |
| **Backend (.NET 10)** | CQRS con MediatR, `FluentValidation`, `ErrorOr<T>`, `[HasPermission(Permissions.Categories.*)]`, EF Core Read / Dapper Write, Reglas Clean Code de Redarbor | Onion Architecture pura (sin `DbContext` en Application), 1 tipo por archivo, métodos <= 25 líneas, paso de objetos |
| **Frontend (Angular 22+)** | Standalone Zoneless, `OnPush`, Signals, `FormGroup`, `*ngxPermissionsOnly`, `permissionGuard`, `@accionadev/ui-kit` (`<ad-table>`, `<ad-modal>`, `<ad-confirm-dialog>`) | Consumo estricto del Design System sin HTML nativo improvisado ni colores arbitrarios |
| **App Mobile** | N/A | Deshabilitado en `PROJECT_PROFILE.md` |

---

## 4. Criterios de Aceptación Refinados (Gherkin Enriquecido)

### Escenario 1 — Registro exitoso de categoría con paginación actualizada (Camino feliz)
- **Dado** que un usuario autenticado con permiso `Categories.Create` y `Categories.Read` abre el módulo `/categories`
- **Cuando** abre el modal de creación, completa el objeto de categoría (`code: "electronica"`, `name: "Electrónica"`, `description: "Equipos y accesorios"`) y guarda
- **Entonces** el backend persiste la categoría vía Dapper con `IsActive = true`, responde `201 Created` con `CategoryResponse`, y la tabla paginada recarga mostrando el registro activo junto con una notificación de éxito

### Escenario 2 — Consulta paginada y filtrada por búsqueda y estado (Caso alternativo)
- **Dado** que existen múltiples categorías activas e inactivas registradas en SQL Server
- **Cuando** un usuario con permiso `Categories.Read` filtra por texto `"elec"` y estado `"Activo"` en la página `1` con tamaño `10`
- **Entonces** `CategoryFinder` consulta con EF Core `.AsNoTracking()` devolviendo únicamente las coincidencias paginadas (`PagedCategoriesResponse`) sin bloquear el hilo UI

### Escenario 3 — Rechazo por código/nombre duplicado o bloqueo por productos asociados (Caso borde / Excepción)
- **Dado** que existe una categoría `"hogar"` con productos asociados y otra categoría `"electronica"`
- **Cuando** un usuario autorizado intenta crear/editar con código o nombre duplicado, o intenta eliminar `"hogar"`
- **Entonces** el sistema rechaza la operación con `409 Conflict` bajo RFC 7807 (`Category.DuplicateCode`, `Category.DuplicateName` o `Category.HasAssociatedProducts`) y muestra el mensaje amigable explicando el motivo o sugiriendo inactivar

### Escenario 4 — Restricción PBAC por permiso granular (Seguridad)
- **Dado** que un usuario autenticado posee únicamente `Categories.Read` (rol `User`) o no posee ningún permiso de categorías
- **Cuando** navega a `/categories` o intenta invocar `POST`, `PUT` o `DELETE` sobre `/api/v1/categories`
- **Entonces** la interfaz oculta los botones protegidos por `*ngxPermissionsOnly`, `permissionGuard` redirige a `/forbidden` si carece de `Categories.Read`, y los endpoints devuelven `403 Forbidden` (RFC 7807) ante peticiones directas sin permiso

---

## 5. Checklist de Preparación para Especificación Técnica

- [x] Todas las preguntas de negocio fueron respondidas (cero marcadores de plantilla pendientes).
- [x] Los permisos (`Categories.Read`, `Categories.Create`, `Categories.Update`, `Categories.Delete`) están alineados con `Domain/Constants/Permissions.cs` y `permissions.constants.ts`.
- [x] Modo Single-Tenant de `PROJECT_PROFILE.md` documentado explícitamente (sin `tenant_id` ni `company_id`).
- [x] Todos los componentes de UI fueron contrastados contra `../ui-design-system/ui-manifest.json` (`<ad-*>`).
- [x] Cumplimiento garantizado de la Regla 7 de Redarbor (objetos como parámetro en Dominio, Repositorios, Comandos, Finders y Frontend) y arquitectura híbrida EF Core Read / Dapper Write sin `FOREIGN KEY`.
- [x] El artefacto está guardado como `specs/US-001_catalogo_de_categorias/US-001-interview.md`.

> **Siguiente Paso**: Redactar `US-001.spec.md`, `US-001-plan.md` y `US-001-tasks.md` tomando esta entrevista y el GitHub Issue #1 como fuente autoritativa.
