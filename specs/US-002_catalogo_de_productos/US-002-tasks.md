# Desglose de Tareas Técnicas (Tasks): US-002 - Catálogo de Productos

**Issue Relacionado**: [US-002 (#3)](https://github.com/walo/prueba_computrabajo/issues/3)  
**Plan de Implementación**: [US-002-plan.md](US-002-plan.md)  

---

## 1. Tareas de Dominio & Permisos PBAC (.NET 10)

- [x] `TASK-01`: Crear ValueObjects / Parameter Objects (`ProductCreationData.cs`, `ProductUpdateData.cs`, `ProductFilter.cs`, `ProductUniquenessFilter.cs`) en `src/backend/src/Core/Domain/ValueObjects/` y eventos de dominio (`ProductCreatedDomainEvent.cs`, `ProductUpdatedDomainEvent.cs`, `ProductDeletedDomainEvent.cs`) en `src/backend/src/Core/Domain/Events/` (1 tipo público por archivo).
- [x] `TASK-02`: **[TDD Red -> Green]** Escribir pruebas unitarias `ProductTests.cs` y crear entidad `Product.cs` en `src/backend/src/Core/Domain/Entities/` con `Create`, `Update`, `AdjustStock`, `IsLowStock`, `MarkDeleted` y normalización de `Sku`/`Name` cumpliendo las 7 Reglas Clean Code Redarbor.
- [x] `TASK-03`: Verificar que `Permissions.Products` (`Read`, `Create`, `Update`, `Delete`) permanecen completos en `Core/Domain/Constants/Permissions.cs` y asignados correctamente en `InitialDataSeeder.cs` (`ADMIN` todos; `USER` solo `Products.Read`).

---

## 2. Tareas de Aplicación CQRS & TDD (.NET 10)

- [x] `TASK-04`: Definir interfaces `IProductRepository.cs` e `IProductFinder.cs` en `src/backend/src/Core/Application/Common/Interfaces/`.
- [x] `TASK-05`: Crear DTOs (`CreateProductRequest.cs`, `UpdateProductRequest.cs`, `ProductResponse.cs`, `PagedProductsResponse.cs`) y contratos Commands/Queries en `src/backend/src/Core/Application/Features/Products/`.
- [x] `TASK-06`: **[TDD Red]** Escribir pruebas unitarias en `tests/Tests.UnitTests/Core/Application/Features/Products/` para `ProductValidatorsTests`, `CreateProductCommandHandlerTests`, `UpdateProductCommandHandlerTests`, `DeleteProductCommandHandlerTests` y `GetProductsQueryHandlerTests`.
- [x] `TASK-07`: **[TDD Green]** Implementar Validadores FluentValidation (`CreateProductCommandValidator`, `UpdateProductCommandValidator`, `DeleteProductCommandValidator`, `GetProductsQueryValidator`, `GetProductByIdQueryValidator`), `ProductWriteSupport` y Handlers (validando `Category` existente/activa vía `ICategoryRepository`, unicidad de `Sku` y bloqueo de borrado con `Stock > 0` o movimientos) sin inyectar `AppDbContext`.

---

## 3. Tareas de Infraestructura SQL Server & Dapper (.NET 10)

- [x] `TASK-08`: Registrar `DbSet<Product> Products` en `AppDbContext.cs`, crear `ProductConfiguration.cs` (tabla `Products`, `Price` `decimal(18,2)`, índice único `IX_Products_Sku`, índice no único `IX_Products_CategoryId`, **cero `.HasForeignKey()`**) y registrar servicios en `Infrastructure/Persistence/DependencyInjection.cs`.
- [x] `TASK-09`: Implementar `ProductFinder.cs` (EF Core `.AsNoTracking()` + `LINQ Join` con `Categories` para `CategoryCode`/`CategoryName` + filtros `Search`/`CategoryId`/`IsActive`/`LowStockOnly`) y `ProductRepository.cs` (escrituras Dapper vía `IDbConnectionFactory` + fallback InMemory + `HasInventoryMovementsAsync` con verificación segura `OBJECT_ID` en 2 pasos).
- [x] `TASK-10`: Generar migración `AddProductsCatalog` (`--configuration Release`), verificar 0 ocurrencias de `ForeignKey` en los archivos generados, y aplicarla contra `redarbor-sqlserver` (`dotnet ef database update --configuration Release`).

---

## 4. Tareas de Presentación `Web.API` & Integración (.NET 10)

- [x] `TASK-11`: Implementar `ProductsController.cs` en `Presentation/Web.API/Controllers/` heredando `ApiController` y decorando cada endpoint con `[HasPermission(Permissions.Products.*)]`.
- [x] `TASK-12`: **[TDD Red -> Green]** Escribir `ProductsIntegrationTests.cs` en `tests/Tests.IntegrationTests/Features/Products/` validando PBAC (`401`/`403`), CRUD completo, paginación con join a Categoría, filtro `lowStockOnly`, rechazo de categoría inactiva (`409`), SKU duplicado (`409`) y borrado bloqueado si `Stock > 0` (`409`), pasando en InMemory y contra SQL Server Docker (`3/3`).

---

## 5. Tareas de Frontend (Angular 22+ Zoneless & `@accionadev/ui-kit`)

- [x] `TASK-13`: Agregar mensajes de error `Product.*` en `src/frontend/src/app/core/config/error-messages.json` y crear modelos TypeScript en `src/frontend/src/app/features/products/models/` (`product.model.ts`, `product-filter.model.ts`, `create-product-request.model.ts`, `update-product-request.model.ts`, `product-update-command.model.ts`, `paged-products-response.model.ts`).
- [x] `TASK-14`: **[TDD Red -> Green]** Escribir `products.service.spec.ts` en Red, implementar `ProductsService` en `src/frontend/src/app/features/products/services/products.service.ts` y verificar paso en Green con Jest.
- [x] `TASK-15`: **[TDD Red -> Green]** Escribir `product-list.component.spec.ts` en Red, implementar `ProductListComponent` (`features/products/pages/product-list.component.ts` + `.html`) usando exclusivamente `@accionadev/ui-kit` (`<ad-card>`, `<ad-table>`, `<ad-modal>`, `<ad-search-box>`, `<ad-select>`, `<ad-switch>`, `<ad-input>`, `<ad-textarea>`, `<ad-badge>`, `<ad-table-actions>`, `<ad-alert>`, `AdConfirmService`) y `*ngxPermissionsOnly`, registrar ruta `/products` en `FEATURES_ROUTES`, menú en `ShellComponent` y enlace en `DashboardComponent`, verificando Jest al 100%.

---

## 6. Cierre y Quality Gate

- [x] `TASK-16`: Ejecutar suite global backend (`dotnet test src/backend/EnterpriseApp.sln --configuration Release`) y frontend (`npm run test:ci && npm run build`), reiniciar daemon `Web.API` y actualizar el grafo AST con Graphify.
