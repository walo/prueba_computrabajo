# Entrevista Técnica y de Negocio: Catálogo de Productos

**Historia ID / GitHub Issue**: US-002  
**Issue URL**: https://github.com/walo/prueba_computrabajo/issues/3  
**Fecha de la Sesión**: 2026-11-15  
**Entrevistador**: Agente IA (Antigravity)  
**Entrevistado (Stakeholder/PO/Dev)**: Walter Urrego (Technical Lead / Candidato Redarbor)  
**Estado Issue**: In Progress (Rama `feature/US-002-catalogo_de_productos`)  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada y gestionada en GitHub Issue #3  

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
El catálogo de Productos constituye la entidad operativa central del sistema de inventario de Redarbor (`Catálogo -> Producto -> Movimiento de Inventario`). Permite a los operadores autorizados registrar, consultar, filtrar, actualizar y depurar productos vinculados lógicamente a una categoría activa de `US-001`, controlando su SKU único, precio unitario, stock inicial, stock mínimo de alerta y estado operativo, sin utilizar restricciones `FOREIGN KEY` en SQL Server conforme a la especificación técnica.

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - Entidad `Product` (`Core/Domain`), CRUD RESTful completo (`/api/v1/products`) con CQRS (EF Core `.AsNoTracking()` para lecturas paginadas/enriquecidas y Dapper para escrituras) y cero `FOREIGN KEY` en SQL Server.
  - Paginación y filtros server-side (`Page`, `PageSize`, `Search` por `Sku`/`Name`, `CategoryId`, `IsActive`, `LowStockOnly`) enriqueciendo `CategoryName` y `CategoryCode` mediante `LINQ Join` de lectura en `ProductFinder`.
  - Validación de existencia y estado activo de la categoría asociada al crear o editar un producto, e impedimento de eliminación física si el producto posee `Stock > 0` o movimientos históricos en `dbo.InventoryMovements`.
  - Protección granular PBAC mediante `Permissions.Products` (`Read`, `Create`, `Update`, `Delete`) en Backend (`HasPermission`) y Frontend (`permissionGuard` y `*ngxPermissionsOnly`).
  - Vista Angular 22+ Zoneless en `/products` construida 100% con componentes de `@accionadev/ui-kit` (`<ad-table>`, `<ad-modal>`, `<ad-select>`, `<ad-input>`, `<ad-textarea>`, `<ad-switch>`, `<ad-search-box>`, `<ad-badge>`, `<ad-table-actions>`, `<ad-confirm-dialog>`).
- **Fuera del alcance explícito (Out-of-Scope)**:
  - Registro de entradas y salidas de inventario (`/api/v1/inventory-movements`, reservado para `US-003`).
  - Ajuste manual directo del `Stock` actual durante la edición de producto (`PUT /api/v1/products/{id}`).
  - Creación de restricciones `FOREIGN KEY` en SQL Server o manejo multi-tenant por `tenant_id` (`Single-Tenant` en `PROJECT_PROFILE.md`).

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

> Cada bloque representa un área de indagación clave resuelta durante la entrevista técnica.

### Bloque A: Dominio y Reglas de Negocio

#### Pregunta A.1: Ciclo de vida del Stock entre creación, edición y movimientos de inventario
- **Contexto y Desafío Identificado**: Al crear un producto puede existir inventario físico inicial, mientras que en producción la prueba exige que el stock cambie mediante un endpoint dedicado de movimientos (`US-003`). Permitir sobrescribir `Stock` en `PUT` rompería la trazabilidad de movimientos.
- **Opciones Evaluadas**:
  - *Opción 1*: Permitir `InitialStock` (`>= 0`, default `0`) en creación (`POST`) y bloquear la modificación directa de `Stock` en edición (`PUT`), dejando todo ajuste posterior a `US-003`.
  - *Opción 2*: Permitir modificar `Stock` arbitrariamente en `PUT /api/v1/products/{id}` o forzar `Stock = 0` al crear.
- **Decisión Acordada**: Opción 1 (`InitialStock` en `POST`; inmutable vía `PUT`).
- **Justificación Técnica & Regla Resultante**: En `CreateProductRequest`, `InitialStock` (entero `>= 0`, defecto `0`) establece el saldo inicial. En `UpdateProductRequest` se actualizan únicamente los metadatos maestros (`Sku`, `Name`, `Description`, `CategoryId`, `Price`, `MinStock`, `IsActive`). La entidad `Product` expone `AdjustStock(int newStock)` para ser consumida exclusivamente por el flujo de movimientos de `US-003`.

#### Pregunta A.2: Reglas de validación de Categoría y política de eliminación
- **Contexto y Desafío Identificado**: No existen `FOREIGN KEY` en la base de datos por requisito explícito de la prueba técnica, y al mismo tiempo `US-001` introdujo `CategoryRepository.HasAssociatedProductsAsync` verificando `dbo.Products`.
- **Opciones Evaluadas**:
  - *Opción 1*: Validar en `Application` que `Category` exista (`404 Product.CategoryNotFound`) y esté activa (`409 Product.CategoryInactive`); y en `DELETE`, impedir el borrado si `Stock > 0` o si existen filas en `dbo.InventoryMovements` (`409 Product.CannotDeleteWithStockOrMovements`).
  - *Opción 2*: Permitir asignar categorías inactivas o realizar únicamente borrado lógico.
- **Decisión Acordada**: Opción 1 (Validación estricta de categoría activa en escritura y bloqueo condicional en eliminación física).
- **Justificación Técnica & Regla Resultante**: La integridad referencial lógica reside en los handlers de `Application` (`ICategoryRepository` y `IProductRepository`). Además, al crear la tabla `dbo.Products` con índice no único `IX_Products_CategoryId`, la validación `HasAssociatedProductsAsync` de `US-001` queda activa automáticamente en SQL Server.

#### Pregunta A.3: Gestión de rama base en Gitflow respecto al PR #2 de `US-001`
- **Contexto y Desafío Identificado**: `US-002` depende directamente de `Category` e `ICategoryRepository` entregados en `US-001` (PR #2 abierto hacia `develop`).
- **Opciones Evaluadas**:
  - *Opción 1*: Hacer merge del PR #2 en GitHub hacia `develop` y ramificar `feature/US-002-catalogo_de_productos` desde `develop` actualizado.
  - *Opción 2*: Mantener el PR #2 abierto y ramificar desde la rama `feature/US-001`.
- **Decisión Acordada**: Opción 1 (Merge de PR #2 a `develop` y creación de `feature/US-002-catalogo_de_productos` desde `develop`).
- **Justificación Técnica & Regla Resultante**: Garantiza historial Gitflow limpio donde cada historia nace de `develop` actualizado y evita conflictos acumulados entre PRs.

---

### Bloque B: Roles, Permisos y Seguridad (PBAC)

#### Pregunta B.1: Permisos PBAC y alcance Single-Tenant aplicables a Productos
- **Contexto**: El catálogo centralizado en `Domain/Constants/Permissions.cs` y `src/frontend/src/app/core/constants/permissions.constants.ts` ya incluye `Permissions.Products` (`Read`, `Create`, `Update`, `Delete`) cargados automáticamente por `InitialDataSeeder` para los roles `ADMIN` (todos) y `USER` (`Products.Read`).
- **Decisión Acordada**:
  - Permisos autorizados para consultar (`GET /api/v1/products`, `GET /api/v1/products/{id}`): `Permissions.Products.Read`.
  - Permisos autorizados para crear (`POST`): `Permissions.Products.Create`.
  - Permisos autorizados para modificar (`PUT`): `Permissions.Products.Update`.
  - Permisos autorizados para eliminar (`DELETE`): `Permissions.Products.Delete`.
- **Alcance de Datos (Single-Tenant)**:
  - Conforme a `PROJECT_PROFILE.md`, la prueba técnica opera en modo **Single-Tenant** con `Nivel 3` (PBAC granular en rutas, menú y endpoints, JWT ultraligero + Refresh Token Rotation SHA-256 + Blacklist O(1) de `jti` en Redis). No aplica aislamiento `tenant_id` ni RLS multi-tenant en las consultas de inventario.

---

### Bloque C: Experiencia de Usuario (UX) e Interacción Visual

#### Pregunta C.1: Flujo de pantalla `/products`, filtros y formularios con `@accionadev/ui-kit`
- **Flujo de Pantalla Acordado**: El usuario con permiso `Products.Read` accede desde el ítem "Productos" en el menú lateral (`ShellComponent`) a `/products`. Ve la barra de herramientas con `<ad-search-box>` (búsqueda por SKU/Nombre), `<ad-select>` de Categoría (cargadas desde `CategoriesService`), `<ad-select>` de Estado (`Todos`, `Activos`, `Inactivos`), `<ad-switch>` o selector para `Solo Stock Bajo`, y botón `+ Nuevo Producto` (protegido por `*ngxPermissionsOnly="['Products.Create']"`). La tabla `<ad-table>` muestra SKU, Nombre, Categoría (`CategoryName`), Precio unitario formateado, Stock vs. Mínimo con `<ad-badge>` semántico (alerta visual `warning`/`danger` cuando `stock <= minStock`), Estado (`Activo`/`Inactivo`) y acciones de fila (`Editar` / `Eliminar` según permisos). La creación y edición ocurren en `<ad-modal>` reutilizable donde el campo `Stock inicial` se muestra al crear y se oculta o queda de solo lectura al editar.
- **Componentes `@accionadev/ui-kit` a Reutilizar**: `<ad-card>`, `<ad-search-box>`, `<ad-select>`, `<ad-switch>`, `<ad-button>`, `<ad-table>`, `ng-template[adTableCell]`, `<ad-table-actions>`, `<ad-badge>`, `<ad-modal>`, `<ad-input>`, `<ad-textarea>`, `<ad-alert>`, `<ad-empty-state>` y `<ad-confirm-dialog />` vía `AdConfirmService`.
- **Nuevos Componentes Requeridos en `ui-kit`**: Ninguno; todos los controles requeridos (`input type="number"`, `icon="dollar"`, `select`, `switch`, `table`, `modal`, `badge`, `confirm-dialog`) existen en `../ui-design-system/ui-manifest.json`.
- **Estados Visuales Obligatorios**:
  - **Carga (Loading)**: Propiedad `[loading]="isLoading()"` de `<ad-table>` y `[loading]="isSaving()"` en el botón de guardado del modal.
  - **Vacío (Empty State)**: `<ad-empty-state>` dentro del template vacío cuando no existan productos o no coincidan con los filtros activos.
  - **Error controlado**: `<ad-alert type="danger">` dentro del modal y banner general mapeando códigos `Product.*` mediante `error-messages.json`.
  - **Restricción de permisos**: Navegación sin `Products.Read` redirige a `/forbidden` (403) mediante `permissionGuard`; botones de mutación ocultos vía `*ngxPermissionsOnly`.

---

### Bloque D: Casos de Borde, Excepciones y Resiliencia

#### Pregunta D.1: Duplicidad de SKU, categoría inexistente/inactiva y bloqueo de borrado
- **Escenario Límite / Falla**:
  1. Envío de `Sku` duplicado (sin importar mayúsculas/minúsculas o espacios).
  2. Referencia a un `CategoryId` inexistente o inactivo.
  3. Intento de eliminar (`DELETE`) un producto cuyo `Stock > 0` o que posee movimientos históricos en `InventoryMovements`.
- **Comportamiento Esperado**: El backend rechaza la operación antes de escribir con **ProblemDetails RFC 7807**, garantizando consistencia de datos aunque no existan `FOREIGN KEY` en SQL Server.
- **Manejo de Errores y Mensajería**:
  - `409 Conflict` (`Product.DuplicateSku`): "Ya existe un producto registrado con ese SKU."
  - `404 Not Found` (`Product.CategoryNotFound`): "La categoría seleccionada no existe."
  - `409 Conflict` (`Product.CategoryInactive`): "No se puede asociar el producto a una categoría inactiva."
  - `409 Conflict` (`Product.CannotDeleteWithStockOrMovements`): "No se puede eliminar un producto con stock mayor a cero o movimientos históricos. Inactívalo en su lugar."
  - `404 Not Found` (`Product.NotFound`): "El producto solicitado no existe o fue eliminado."

---

### Bloque E: Persistencia, Integración y Eventos

#### Pregunta E.1: Esquema `dbo.Products`, CQRS Dual (EF Core + Dapper) y enlace con Categorías
- **Estrategia de Persistencia**: Nueva tabla `dbo.Products` generada mediante migración EF Core `AddProductsCatalog` sin ninguna restricción `FOREIGN KEY`. Índices explícitos: único `IX_Products_Sku` (`nvarchar(30)`) y no único `IX_Products_CategoryId` (`uniqueidentifier`) para consultas rápidas por categoría y para acelerar `CategoryRepository.HasAssociatedProductsAsync`.
- **Lectura / Consulta (`ProductFinder`)**: Implementa `IProductFinder` usando **EF Core `.AsNoTracking()`** y `LINQ Join` entre `dbContext.Products` y `dbContext.Categories` por `CategoryId == Category.Id` para retornar `CategoryName` y `CategoryCode`, aplicando filtros (`Search`, `CategoryId`, `IsActive`, `LowStockOnly`) y paginación en SQL Server (`ORDER BY Name`, `Skip`, `Take`).
- **Escritura / Dominio (`ProductRepository`)**: Implementa `IProductRepository` usando **Dapper** (`IDbConnectionFactory`) para `INSERT`, `UPDATE` y `DELETE` parametrizados en modo relacional (con respaldo EF Core InMemory para pruebas rápidas si `!dbContext.Database.IsRelational()`), además del método seguro en 2 pasos (`OBJECT_ID(N'dbo.InventoryMovements', N'U')`) para `HasInventoryMovementsAsync`.
- **Eventos de Dominio**: `ProductCreatedDomainEvent`, `ProductUpdatedDomainEvent` y `ProductDeletedDomainEvent` registrados en el agregado `Product` (`Core/Domain/Events/`).

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (SQL Server 2022)** | Tabla `dbo.Products` (`Decimal(18,2)` para `Price`, índices `IX_Products_Sku` UNIQUE y `IX_Products_CategoryId` NONCLUSTERED, `0` Foreign Keys) | Requisito explícito de la prueba (0 FKs), precisión financiera en precios y consultas rápidas de filtro y conteo |
| **Backend (.NET 10)** | CQRS MediatR + FluentValidation + ErrorOr + `ProductCreationData` / `ProductUpdateData` / `ProductFilter` + `[HasPermission(Permissions.Products.*)]` | Onion Architecture pura (sin `DbContext` en Application), EF Core `.AsNoTracking()` para lecturas, Dapper para escrituras, 7 Reglas Clean Code Redarbor |
| **Frontend (Angular 22+)** | Standalone `OnPush`, Signals, `ProductsService`, `<ad-table>`, `<ad-modal>`, `<ad-select>`, `<ad-search-box>`, `*ngxPermissionsOnly` | Zoneless puro, cumplimiento estricto de `@accionadev/ui-kit` y PBAC dinámico |
| **App Mobile** | N/A | Deshabilitado en `PROJECT_PROFILE.md` para esta prueba técnica |

---

## 4. Criterios de Aceptación Refinados (Gherkin Enriquecido)

> Estos escenarios consolidan los criterios de aceptación del GitHub Issue #3 con las definiciones técnicas acordadas.

### Escenario 1 — Creación de producto con categoría activa y stock inicial (Camino Feliz)
- **Dado** un usuario autenticado con permiso `Products.Create` y una categoría activa existente (`Id = cat-1`, `Code = 'elec'`, `Name = 'Electrónica'`)
- **Cuando** envía `POST /api/v1/products` con `{ sku: " prod-001 ", name: "Laptop Pro 14", categoryId: "cat-1", price: 1299.99, initialStock: 10, minStock: 3 }`
- **Entonces** el sistema normaliza el SKU a `"PROD-001"`, valida la existencia y estado activo de `cat-1`, persiste el registro mediante Dapper en `dbo.Products`, y responde `201 Created` con `ProductResponse` incluyendo `stock = 10`, `isLowStock = false`, `categoryCode = "elec"` y `categoryName = "Electrónica"`.

### Escenario 2 — Consulta paginada con filtros combinados e indicador de stock bajo
- **Dado** un usuario autenticado con permiso `Products.Read` y productos registrados en distintas categorías
- **Cuando** solicita `GET /api/v1/products?page=1&pageSize=10&categoryId=cat-1&lowStockOnly=true` o interactúa con los filtros de la pantalla `/products`
- **Entonces** `ProductFinder` consulta con EF Core `.AsNoTracking()` haciendo join lógico con `Categories`, filtra por categoría y `Stock <= MinStock`, y responde `200 OK` con `PagedProductsResponse` paginado mostrando los badges de alerta correspondientes en `<ad-table>`.

### Escenario 3 — Validación de reglas de negocio e integridad lógica sin FKs (Excepciones)
- **Dado** un usuario autorizado con permisos de mutación (`Products.Create`, `Products.Update` o `Products.Delete`)
- **Cuando** intenta crear un producto con un SKU ya existente (case-insensitive), asignarlo a una categoría inactiva/inexistente, o eliminar un producto que tiene `Stock > 0` o movimientos históricos en `InventoryMovements`
- **Entonces** el sistema rechaza la operación devolviendo ProblemDetails RFC 7807 con código `409 Conflict` (`Product.DuplicateSku`, `Product.CategoryInactive`, `Product.CannotDeleteWithStockOrMovements`) o `404 Not Found` (`Product.CategoryNotFound`) y la UI traduce el mensaje amigablemente en `<ad-alert>`.

### Escenario 4 — Protección PBAC de endpoints y acciones UI
- **Dado** un usuario autenticado cuyo rol solo tiene `Products.Read` (como `USER`) o carece de permisos sobre Productos
- **Cuando** navega a `/products` o intenta invocar `POST/PUT/DELETE /api/v1/products`
- **Entonces** un usuario con `Products.Read` puede consultar el catálogo pero no ve los botones Crear/Editar/Eliminar (`*ngxPermissionsOnly`) y recibe `403 Forbidden` si intenta llamar a la API de escritura; un usuario sin `Products.Read` es redirigido a `/forbidden` (`403`) y no ve el enlace "Productos" en el menú lateral.

---

## 5. Checklist de Preparación para Especificación Técnica

- [x] Todas las preguntas de negocio fueron respondidas (sin marcadores pendientes ni ambigüedades).
- [x] Los permisos especificados (`Products.Read`, `Products.Create`, `Products.Update`, `Products.Delete`) existen en `Permissions.cs` y `permissions.constants.ts`.
- [x] El perfil Single-Tenant + Level 3 PBAC y la regla de 0 `FOREIGN KEY` están respetados explícitamente.
- [x] Los componentes de UI fueron verificados contra `../ui-design-system/ui-manifest.json` (`@accionadev/ui-kit`).
- [x] La arquitectura respeta Onion puro (`IProductRepository`, `ICategoryRepository`, `IProductFinder`, `IUnitOfWork` sin `DbContext` en Application) y separación CQRS (EF Core lectura / Dapper escritura).
- [x] El artefacto está guardado como `specs/US-002_catalogo_de_productos/US-002-interview.md`.

> **Siguiente Paso**: Redactar la trilogía técnica `US-002.spec.md`, `US-002-plan.md` y `US-002-tasks.md` en `specs/US-002_catalogo_de_productos/`.
