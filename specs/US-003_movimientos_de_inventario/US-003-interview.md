# Entrevista Técnica y de Negocio: Gestión de Movimientos de Inventario

**Historia ID / GitHub Issue**: US-003  
**Issue URL**: https://github.com/walo/prueba_computrabajo/issues/5  
**Fecha de la Sesión**: 2026-11-15  
**Entrevistador**: Agente IA (Antigravity)  
**Entrevistado (Stakeholder/PO/Dev)**: Walter Urrego (Technical Lead / Candidato Redarbor)  
**Estado Issue**: In Progress (Rama `feature/US-003-movimientos_de_inventario`)  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada y gestionada en GitHub Issue #5  

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
El módulo de **Movimientos de Inventario** completa el flujo operativo del sistema de inventario Redarbor (`Categoría -> Producto -> Movimiento de Inventario`). Permite a los operadores autorizados registrar entradas (`ENTRY`) y salidas (`EXIT`) sobre productos activos del catálogo (`US-002`) y consultar el libro mayor histórico inmutable (*Kardex*), garantizando trazabilidad exacta de existencias (`PreviousStock`, `NewStock`), responsable autenticado (`PerformedByUserId`) y fecha/hora UTC sin utilizar restricciones `FOREIGN KEY` en SQL Server.

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - Entidad de dominio `InventoryMovement` (`Core/Domain`), inmutable tras su creación, con validación de tipos (`ENTRY` / `EXIT`), cantidad (`>= 1`), motivo (`3`–`300` caracteres) y prevención estricta de stock negativo (`NewStock >= 0`).
  - Integración transaccional de escritura con **Dapper** (`INSERT` en `dbo.InventoryMovements` + actualización de `Product.Stock` vía `IProductRepository.UpdateAsync`) tomando `PerformedByUserId` exclusivamente desde `ICurrentUserContext.UserId`.
  - Lectura paginada y filtrada en `InventoryMovementFinder` (**EF Core `.AsNoTracking()`**) con `GroupJoin` / `DefaultIfEmpty` lógico hacia `Products` y `Users` para enriquecer `ProductSku`, `ProductName` y `PerformedByEmail`, filtrando por `Search`, `ProductId`, `Type`, `FromDateUtc` y `ToDateUtc`.
  - Activación de la validación cruzada en `US-002` (`ProductRepository.HasInventoryMovementsAsync` en modo relacional e InMemory) impidiendo eliminar productos con historial en `InventoryMovements`.
  - Protección granular PBAC mediante `Permissions.InventoryMovements.Read` y `Permissions.InventoryMovements.Create` en Backend y Frontend.
  - Vista Angular 22+ Zoneless en `/inventory-movements` construida 100% con `@accionadev/ui-kit` (`<ad-card>`, `<ad-table>`, `<ad-search-box>`, `<ad-select>`, `<ad-input>`, `<ad-textarea>`, `<ad-badge>`, `<ad-button>`, `<ad-modal>`, `<ad-alert>`, `<ad-empty-state>`) y `*ngxPermissionsOnly`.
- **Fuera del alcance explícito (Out-of-Scope)**:
  - Endpoints `PUT`, `PATCH` o `DELETE` sobre movimientos históricos (el Kardex es estrictamente append-only; las correcciones operativas se realizan con un movimiento compensatorio de signo contrario).
  - Transferencias multi-bodega o lotes con fecha de vencimiento.
  - Creación de restricciones `FOREIGN KEY` en SQL Server o aislamiento multi-tenant (`Single-Tenant` en `PROJECT_PROFILE.md`).

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

> Cada bloque representa un área de indagación clave resuelta durante la entrevista técnica y arquitectónica.

### Bloque A: Dominio y Reglas de Negocio

#### Pregunta A.1: Gestión de rama base en Gitflow respecto al PR #4 de `US-002`
- **Contexto y Desafío Identificado**: `US-003` depende directamente de la entidad `Product`, `IProductRepository`, `ProductRepository.HasInventoryMovementsAsync` y `Permissions.InventoryMovements` integrados en `US-002`.
- **Opciones Evaluadas**:
  - *Opción 1*: Hacer merge del PR #4 (`feature/US-002-catalogo_de_productos` -> `develop`) y crear `feature/US-003-movimientos_de_inventario` desde `develop` actualizado.
  - *Opción 2*: Mantener el PR #4 abierto y ramificar desde `feature/US-002`.
- **Decisión Acordada**: Opción 1 (Merge de PR #4 a `develop` y creación de `feature/US-003-movimientos_de_inventario` desde `develop`).
- **Justificación Técnica & Regla Resultante**: Mantiene trazabilidad limpia en Gitflow y evita dependencias encadenadas entre ramas de feature.

#### Pregunta A.2: Tipos de movimiento, cálculo de saldos y estado del producto
- **Contexto y Desafío Identificado**: Se requiere asegurar consistencia entre el saldo del producto y el Kardex sin exponer `PreviousStock`, `NewStock` o `PerformedByUserId` a manipulación desde el cliente HTTP.
- **Opciones Evaluadas**:
  - *Opción 1*: Capturar del usuario únicamente `ProductId`, `Type` (`ENTRY` | `EXIT`), `Quantity` (`>= 1`) y `Reason` (`3`–`300` caracteres); calcular `PreviousStock` y `NewStock` en servidor desde el `Product` activo actual y resolver `PerformedByUserId` desde el JWT (`ICurrentUserContext`).
  - *Opción 2*: Permitir enviar saldos o ajustes arbitrarios desde el cliente sin validar el estado activo del producto.
- **Decisión Acordada**: Opción 1 (Cálculo autoritativo en dominio/aplicación y rechazo de productos inactivos o saldos negativos).
- **Justificación Técnica & Regla Resultante**: `InventoryMovement.Create` normaliza `Type` a mayúsculas (`ENTRY` / `EXIT`), calcula `NewStock = Type == ENTRY ? PreviousStock + Quantity : PreviousStock - Quantity` y exige `NewStock >= 0`. El handler valida `Product` existente (`404`), estado `IsActive` (`409`) y ejecuta `product.AdjustStock(movement.NewStock)`.

---

### Bloque B: Roles, Permisos y Seguridad (PBAC)

#### Pregunta B.1: Permisos PBAC y alcance Single-Tenant aplicables a Movimientos de Inventario
- **Contexto**: El catálogo centralizado en `Domain/Constants/Permissions.cs` y `src/frontend/src/app/core/constants/permissions.constants.ts` ya define `Permissions.InventoryMovements.Read` y `Permissions.InventoryMovements.Create`, sembrados por `InitialDataSeeder` en `ADMIN` (ambos) y `USER` (`InventoryMovements.Read`).
- **Decisión Acordada**:
  - Permiso autorizado para consultar el Kardex (`GET /api/v1/inventory-movements`, `GET /api/v1/inventory-movements/{id}`): `Permissions.InventoryMovements.Read`.
  - Permiso autorizado para registrar movimientos (`POST /api/v1/inventory-movements`): `Permissions.InventoryMovements.Create`.
  - Auditoría de responsable: `PerformedByUserId` y `PerformedByEmail` se derivan estrictamente de `ICurrentUserContext.UserId` autenticado en el token JWT (jamás del payload del request).
- **Alcance de Datos (Single-Tenant)**:
  - Conforme a `PROJECT_PROFILE.md`, la aplicación opera en modo **Single-Tenant** con `Nivel 3` PBAC; no aplica `tenant_id` ni aislamiento RLS organizacional en el Kardex.

---

### Bloque C: Experiencia de Usuario (UX) e Interacción Visual

#### Pregunta C.1: Flujo de pantalla `/inventory-movements`, filtros de fecha y modal de registro con `@accionadev/ui-kit`
- **Flujo de Pantalla Acordado**: El usuario con permiso `InventoryMovements.Read` ingresa desde el ítem `"Movimientos"` en `ShellComponent` (o desde la tarjeta de Movimientos en `DashboardComponent`) hacia `/inventory-movements`. Dispone de barra de filtros con `<ad-search-box>` (búsqueda por SKU, nombre de producto o motivo), `<ad-select>` de Producto, `<ad-select>` de Tipo (`Todos`, `Entradas`, `Salidas`), dos campos `<ad-input type="text" prefixIcon="calendar" placeholder="AAAA-MM-DD">` (`Desde` y `Hasta`) y botón `+ Registrar Movimiento` protegido con `*ngxPermissionsOnly="['InventoryMovements.Create']"`. La grilla `<ad-table>` muestra Fecha/Hora UTC formateada, Producto (`Sku` + `Name`), Tipo (`<ad-badge variant="success">Entrada</ad-badge>` / `<ad-badge variant="warning">Salida</ad-badge>`), Cantidad con signo, Stock Anterior ➔ Stock Resultante, Motivo y Usuario Responsable, sin botones de edición o eliminación. El botón abre `<ad-modal>` con `<ad-select>` de productos activos (`Name (SKU) — Stock: N`), selector de Tipo (`ENTRY` / `EXIT`), `<ad-input type="number">` Cantidad (`>= 1`), tarjeta/resumen reactivo con vista previa de stock resultante (alertando visualmente si `EXIT` supera el stock disponible o si activa stock bajo) y `<ad-textarea>` para Motivo obligatorio.
- **Componentes `@accionadev/ui-kit` a Reutilizar**: `<ad-card>`, `<ad-search-box>`, `<ad-select>`, `<ad-input>` (`type="text"` con `prefixIcon="calendar"` para fechas `AAAA-MM-DD` y `type="number"` para cantidad), `<ad-textarea>`, `<ad-button>`, `<ad-table>`, `<ad-badge>`, `<ad-modal>`, `<ad-alert>` y `<ad-empty-state>`.
- **Nuevos Componentes Requeridos en `ui-kit`**: Ninguno; `<ad-input prefixIcon="calendar">` cubre la captura de fechas en formato ISO `AAAA-MM-DD` sin crear componentes nativos en `src/frontend/` ni modificar `ui-kit`.
- **Estados Visuales Obligatorios**:
  - **Carga (Loading)**: `[loading]="isLoading()"` en `<ad-table>` y `[loading]="isSaving()"` en el botón confirmar del modal.
  - **Vacío (Empty State)**: `<ad-empty-state>` dentro del template vacío de `<ad-table>` cuando el Kardex esté vacío o sin coincidencias para los filtros activos.
  - **Error controlado**: `<ad-alert type="danger">` en modal o pantalla traduciendo códigos `InventoryMovement.*` desde `error-messages.json`.
  - **Restricción de permisos**: Navegación sin `InventoryMovements.Read` redirige a `/forbidden` (`403`) vía `permissionGuard`; botón de registro oculto con `*ngxPermissionsOnly` para usuarios sin `InventoryMovements.Create`.

---

### Bloque D: Casos de Borde, Excepciones y Resiliencia

#### Pregunta D.1: Producto inexistente/inactivo, salida superior al stock disponible e inmutabilidad
- **Escenario Límite / Falla**:
  1. Registro sobre un `ProductId` que no existe (`404`) o está inactivo (`409`).
  2. Salida (`EXIT`) cuya `Quantity` supera el `Product.Stock` disponible (`NewStock < 0`).
  3. Envío de `Type` inválido, `Quantity <= 0`, `Reason` vacío/menor a 3 caracteres, o rango de fechas invertido (`FromDateUtc > ToDateUtc`).
  4. Intento de modificar o eliminar un movimiento histórico (inexistencia intencional de endpoints `PUT`/`DELETE`).
- **Comportamiento Esperado**: El backend rechaza con `ValidationProblemDetails` o `ProblemDetails` RFC 7807 antes de ejecutar escrituras en SQL Server o modificar `Product.Stock`.
- **Manejo de Errores y Mensajería**:
  - `404 Not Found` (`InventoryMovement.ProductNotFound`): "El producto seleccionado no existe o fue eliminado."
  - `409 Conflict` (`InventoryMovement.ProductInactive`): "No se pueden registrar movimientos sobre un producto inactivo."
  - `409 Conflict` (`InventoryMovement.InsufficientStock`): "El stock disponible es insuficiente para realizar la salida solicitada."
  - `404 Not Found` (`InventoryMovement.NotFound`): "El movimiento de inventario solicitado no existe."
  - `400 Bad Request` (`Validation.Failed`): Validaciones de `Type`, `Quantity`, `Reason`, `ProductId` y rango `FromDateUtc <= ToDateUtc`.

---

### Bloque E: Persistencia, Integración y Eventos

#### Pregunta E.1: Esquema `dbo.InventoryMovements`, CQRS Dual (EF Core + Dapper) e integridad cruzada con Productos
- **Estrategia de Persistencia**: Nueva tabla `dbo.InventoryMovements` generada mediante migración EF Core `AddInventoryMovements` sin ninguna restricción `FOREIGN KEY`. Índices explícitos: `IX_InventoryMovements_ProductId` y `IX_InventoryMovements_CreatedAtUtc`.
- **Lectura / Consulta (`InventoryMovementFinder`)**: Implementa `IInventoryMovementFinder` usando **EF Core `.AsNoTracking()`** con `GroupJoin` / `DefaultIfEmpty` lógico hacia `dbContext.Products` (`ProductId == Product.Id`) y `dbContext.Users` (`PerformedByUserId == User.Id`) para proyectar `ProductSku`, `ProductName` y `PerformedByEmail`, filtrando por `Search`, `ProductId`, `Type`, `FromDateUtc`, `ToDateUtc`, ordenado descendentemente por `CreatedAtUtc` y paginado en servidor.
- **Escritura / Dominio (`InventoryMovementRepository` & `ProductRepository`)**: Implementa `IInventoryMovementRepository` usando **Dapper** (`IDbConnectionFactory`) para `INSERT INTO InventoryMovements (...)` (con respaldo EF Core InMemory si `!dbContext.Database.IsRelational()`), coordinado junto con `productRepository.UpdateAsync(product, ct)` y `unitOfWork.SaveChangesAsync(ct)`. Adicionalmente, `ProductRepository.HasInventoryMovementsAsync` actualiza su ruta no relacional para consultar `dbContext.InventoryMovements.AsNoTracking().AnyAsync(...)`.
- **Eventos de Dominio**: `InventoryMovementCreatedDomainEvent` emitido desde el agregado `InventoryMovement`.

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (SQL Server 2022)** | Tabla `dbo.InventoryMovements` (`IX_InventoryMovements_ProductId`, `IX_InventoryMovements_CreatedAtUtc`, `0` Foreign Keys) | Requisito explícito de la prueba (0 FKs), consultas rápidas de Kardex por producto y fecha, y activación nativa de `HasInventoryMovementsAsync` |
| **Backend (.NET 10)** | CQRS MediatR + FluentValidation + ErrorOr + `InventoryMovementCreationData` / `InventoryMovementFilter` + `[HasPermission(Permissions.InventoryMovements.*)]` | Onion Architecture pura (sin `DbContext` en Application), EF Core `.AsNoTracking()` Join para lecturas, Dapper para escrituras y 7 Reglas Clean Code Redarbor |
| **Frontend (Angular 22+)** | Standalone `OnPush`, Signals, `InventoryMovementsService`, `<ad-table>`, `<ad-modal>`, `<ad-select>`, `<ad-input>`, `<ad-badge>`, `*ngxPermissionsOnly` | Zoneless puro, cumplimiento estricto de `@accionadev/ui-kit` y PBAC dinámico |
| **App Mobile** | N/A | Deshabilitado en `PROJECT_PROFILE.md` para esta prueba técnica |

---

## 4. Criterios de Aceptación Refinados (Gherkin Enriquecido)

> Estos escenarios consolidan los criterios de aceptación del GitHub Issue #5 con las definiciones técnicas acordadas.

### Escenario 1 — Registro de Entrada y actualización inmediata del Stock del Producto (Camino Feliz)
- **Dado** un usuario autenticado con permiso `InventoryMovements.Create` (`Id = usr-1`) y un producto activo existente (`Id = prod-1`, `Sku = "LAP-001"`, `Stock = 10`)
- **Cuando** envía `POST /api/v1/inventory-movements` con `{ productId: "prod-1", type: "ENTRY", quantity: 5, reason: "Reposición de almacén proveedor" }`
- **Entonces** el sistema crea `InventoryMovement` con `PreviousStock = 10`, `NewStock = 15`, `PerformedByUserId = usr-1`, ejecuta `product.AdjustStock(15)`, persiste movimiento y producto vía Dapper, y responde `201 Created` con `InventoryMovementResponse`.

### Escenario 2 — Registro de Salida válida, cálculo de alerta Stock Bajo y bloqueo posterior de borrado del Producto (Flujo Secundario)
- **Dado** un usuario autenticado con permiso `InventoryMovements.Create` y un producto activo con `Stock = 10` y `MinStock = 3`
- **Cuando** registra un movimiento `EXIT` por `quantity = 8` con motivo `"Despacho pedido #450"`
- **Entonces** el sistema actualiza `Product.Stock` a `2` (`IsLowStock() == true`), persiste el movimiento con `PreviousStock = 10` y `NewStock = 2`, e impide posteriormente eliminar físicamente el producto en `DELETE /api/v1/products/{id}` devolviendo `409 Conflict` (`Product.CannotDeleteWithStockOrMovements`).

### Escenario 3 — Rechazo por Stock Insuficiente o Producto Inactivo (Excepción / Límite de Negocio)
- **Dado** un usuario autorizado con permiso `InventoryMovements.Create`
- **Cuando** intenta registrar un movimiento `EXIT` cuya cantidad supera el `Stock` disponible del producto, o intenta registrar cualquier movimiento sobre un producto inactivo (`IsActive = false`), o solicita un rango de fechas inválido (`FromDateUtc > ToDateUtc`)
- **Entonces** el sistema rechaza la operación con RFC 7807 (`409 InventoryMovement.InsufficientStock`, `409 InventoryMovement.ProductInactive` o `400 Validation.Failed`), conserva intacto el stock del producto y la UI muestra el error traducido en `<ad-alert>`.

### Escenario 4 — Consulta filtrada del Kardex inmutable y restricción PBAC para solo lectura (Seguridad)
- **Dado** un usuario autenticado con rol `USER` (únicamente `InventoryMovements.Read`) u otro usuario sin permisos de Movimientos
- **Cuando** consulta `GET /api/v1/inventory-movements?productId=...&type=EXIT&fromDateUtc=...` o accede a `/inventory-movements`, o intenta ejecutar `POST /api/v1/inventory-movements` sin `InventoryMovements.Create`
- **Entonces** el usuario con `InventoryMovements.Read` visualiza el Kardex paginado y filtrado sin botones de creación, edición o eliminación (`*ngxPermissionsOnly`) y recibe `403 Forbidden` al intentar crear vía API; un usuario sin `InventoryMovements.Read` es redirigido a `/forbidden` (`403`) y no ve el acceso `"Movimientos"` en el menú lateral.

---

## 5. Checklist de Preparación para Especificación Técnica

- [x] Todas las preguntas de negocio fueron respondidas (sin marcadores pendientes ni ambigüedades).
- [x] Los permisos especificados (`InventoryMovements.Read`, `InventoryMovements.Create`) existen en `Permissions.cs` y `permissions.constants.ts`.
- [x] El perfil Single-Tenant + Level 3 PBAC y la regla de 0 `FOREIGN KEY` están respetados explícitamente.
- [x] Los componentes de UI fueron verificados contra `../ui-design-system/ui-manifest.json` (`@accionadev/ui-kit`).
- [x] La arquitectura respeta Onion puro (`IInventoryMovementRepository`, `IProductRepository`, `IInventoryMovementFinder`, `IUnitOfWork` sin `DbContext` en Application) y separación CQRS (EF Core lectura / Dapper escritura).
- [x] El artefacto está guardado como `specs/US-003_movimientos_de_inventario/US-003-interview.md`.

> **Siguiente Paso**: Redactar la trilogía técnica `US-003.spec.md`, `US-003-plan.md` y `US-003-tasks.md` en `specs/US-003_movimientos_de_inventario/`.
