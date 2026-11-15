# Desglose de Tareas Técnicas (Tasks): US-003 - Movimientos de Inventario

**Issue Relacionado**: [US-003 (#5)](https://github.com/walo/prueba_computrabajo/issues/5)  
**Plan de Implementación**: [US-003-plan.md](US-003-plan.md)  

---

## 1. Tareas de Dominio & Permisos PBAC (.NET 10)

- [x] `TASK-01`: Crear `InventoryMovementTypes.cs` en `src/backend/src/Core/Domain/Constants/`, ValueObjects (`InventoryMovementCreationData.cs`, `InventoryMovementFilter.cs`) en `Core/Domain/ValueObjects/` y `InventoryMovementCreatedDomainEvent.cs` en `Core/Domain/Events/` (1 tipo público por archivo).
- [x] `TASK-02`: **[TDD Red -> Green]** Escribir pruebas unitarias `InventoryMovementTests.cs` y crear entidad inmutable `InventoryMovement.cs` en `Core/Domain/Entities/` validando `Type`, `Quantity >= 1`, `Reason` (`3..300`), cálculo de `NewStock` y rechazo `InventoryMovement.InsufficientStock`, cumpliendo las 7 Reglas Clean Code Redarbor.
- [x] `TASK-03`: Verificar que `Permissions.InventoryMovements` (`Read`, `Create`) permanecen completos en `Core/Domain/Constants/Permissions.cs` y sembrados correctamente en `InitialDataSeeder.cs` (`ADMIN` todos; `USER` solo `InventoryMovements.Read`).

---

## 2. Tareas de Aplicación CQRS & TDD (.NET 10)

- [x] `TASK-04`: Definir interfaces `IInventoryMovementRepository.cs` e `IInventoryMovementFinder.cs` en `src/backend/src/Core/Application/Common/Interfaces/`.
- [x] `TASK-05`: Crear DTOs (`CreateInventoryMovementRequest.cs`, `InventoryMovementResponse.cs`, `PagedInventoryMovementsResponse.cs`) y contratos Commands/Queries en `src/backend/src/Core/Application/Features/InventoryMovements/`.
- [x] `TASK-06`: **[TDD Red]** Escribir pruebas unitarias en `tests/Tests.UnitTests/Core/Application/Features/InventoryMovements/` para `InventoryMovementValidatorsTests` (incluyendo inputs `null`), `CreateInventoryMovementCommandHandlerTests`, `GetInventoryMovementsQueryHandlerTests` y `GetInventoryMovementByIdQueryHandlerTests`.
- [x] `TASK-07`: **[TDD Green]** Implementar Validadores FluentValidation (con guardas `When(x => x.Request is not null, ...)` y `When(x => x.Filter is not null, ...)`) y Handlers (validando usuario en `ICurrentUserContext`, producto existente/activo, cálculo/ajuste de stock y persistencia vía repositorios + `IUnitOfWork`) sin inyectar `AppDbContext`.

---

## 3. Tareas de Infraestructura SQL Server & Dapper (.NET 10)

- [x] `TASK-08`: Registrar `DbSet<InventoryMovement> InventoryMovements` en `AppDbContext.cs`, crear `InventoryMovementConfiguration.cs` (tabla `InventoryMovements`, índices `IX_InventoryMovements_ProductId` y `IX_InventoryMovements_CreatedAtUtc`, **cero `.HasForeignKey()`**) y registrar servicios en `Infrastructure/Persistence/DependencyInjection.cs`.
- [x] `TASK-09`: Implementar `InventoryMovementFinder.cs` (EF Core `.AsNoTracking()` + `GroupJoin`/`DefaultIfEmpty` con `Products` y `Users` + filtros `Search`/`ProductId`/`Type`/`FromDateUtc`/`ToDateUtc` + orden descendente por `CreatedAtUtc`), `InventoryMovementRepository.cs` (escritura Dapper + fallback InMemory) y habilitar la verificación InMemory de `InventoryMovements` en `ProductRepository.HasInventoryMovementsAsync`.
- [x] `TASK-10`: Generar migración `AddInventoryMovements` (`--configuration Release`), verificar `0` ocurrencias de `ForeignKey` en los archivos generados, y aplicarla contra `redarbor-sqlserver` (`dotnet ef database update --configuration Release`).

---

## 4. Tareas de Presentación `Web.API` & Integración (.NET 10)

- [x] `TASK-11`: Implementar `InventoryMovementsController.cs` en `Presentation/Web.API/Controllers/` heredando `ApiController` y decorando cada endpoint con `[HasPermission(Permissions.InventoryMovements.*)]` (`GET` list, `GET` by id, `POST` create — sin `PUT`/`DELETE`).
- [x] `TASK-12`: **[TDD Red -> Green]** Escribir `InventoryMovementsIntegrationTests.cs` en `tests/Tests.IntegrationTests/Features/InventoryMovements/` validando PBAC (`401`/`403` para `USER` en `POST`), flujo completo `ENTRY`/`EXIT` con actualización de `Product.Stock`, rechazo por stock insuficiente (`409`), rechazo por producto inactivo (`409`), bloqueo de eliminación física de producto con historial de movimientos (`409`) y consulta paginada con filtros de fecha/producto/tipo.

---

## 5. Tareas de Frontend (Angular 22+ Zoneless & `@accionadev/ui-kit`)

- [x] `TASK-13`: Agregar mensajes de error `InventoryMovement.*` en `src/frontend/src/app/core/config/error-messages.json` y crear modelos TypeScript en `src/frontend/src/app/features/inventory-movements/models/`.
- [x] `TASK-14`: **[TDD Red -> Green]** Escribir `inventory-movements.service.spec.ts` en Red, implementar `InventoryMovementsService` en `src/frontend/src/app/features/inventory-movements/services/inventory-movements.service.ts` y verificar paso en Green con Jest.
- [x] `TASK-15`: **[TDD Red -> Green]** Escribir `inventory-movement-list.component.spec.ts` en Red, implementar `InventoryMovementListComponent` (`features/inventory-movements/pages/inventory-movement-list.component.ts` + `.html`) usando exclusivamente `@accionadev/ui-kit` y `*ngxPermissionsOnly` (sin botones editar/eliminar y con vista previa de stock resultante), registrar ruta `/inventory-movements` en `FEATURES_ROUTES`, ítem `'Movimientos'` en `ShellComponent` y enlace en `DashboardComponent`, verificando Jest al 100%.

---

## 6. Cierre y Quality Gate

- [x] `TASK-16`: Ejecutar suite global backend (`dotnet test src/backend/EnterpriseApp.sln --configuration Release`) y frontend (`npm run test:ci && npm run build`), reiniciar daemon `Web.API` y actualizar el grafo AST con `graphify update .`.
