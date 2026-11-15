# Desglose de Tareas Técnicas (Tasks): US-001 - Catálogo de Categorías de Productos

**Issue Relacionado**: [US-001](https://github.com/walo/prueba_computrabajo/issues/1)  
**Plan de Implementación**: [US-001-plan.md](US-001-plan.md)  

---

## 1. Tareas de Base de Datos & Dominio (.NET 10)

- [x] `TASK-01`: Crear entidad `Category`, parameter objects (`CategoryCreationData`, `CategoryUpdateData`, `CategoryUniquenessFilter`, `CategoryFilter`) y eventos de dominio en `src/backend/src/Core/Domain/` (1 tipo público por archivo, reglas Clean Code Redarbor).
- [x] `TASK-02`: Definir interfaces `ICategoryRepository` y `ICategoryFinder` en `src/backend/src/Core/Application/Common/Interfaces/`.
- [x] `TASK-03`: Registrar `DbSet<Category>` en `AppDbContext`, crear `CategoryConfiguration.cs` (índices únicos en `Code` y `Name`, cero `.HasForeignKey()`), implementar `CategoryFinder.cs` (EF Core `.AsNoTracking()`), implementar `CategoryRepository.cs` (Dapper escrituras + `HasAssociatedProductsAsync` seguro sobre `dbo.Products`) y registrar en `Infrastructure/Persistence/DependencyInjection.cs`.
- [x] `TASK-04`: Generar migración `AddCategoriesCatalog`, verificar 0 ocurrencias de `ForeignKey`, y aplicarla a `redarbor-sqlserver` (`dotnet ef database update`).

---

## 2. Tareas de Backend (.NET 10 CQRS + TDD)

- [x] `TASK-05`: **[TDD Red]** Escribir pruebas unitarias en `tests/Tests.UnitTests/` para `CategoryTests`, validadores FluentValidation (`Create`, `Update`, `GetCategories`) y handlers (`Create`, `Update`, `Delete`, `GetCategories`, `GetCategoryById`).
- [x] `TASK-06`: Crear DTOs (`CreateCategoryRequest`, `UpdateCategoryRequest`, `CategoryResponse`, `PagedCategoriesResponse`) y Commands/Queries en `Core/Application/Features/Categories/`.
- [x] `TASK-07`: Implementar Validators (`FluentValidation`) y Handlers (inyectando `ICategoryRepository`, `ICategoryFinder`, `IUnitOfWork`, nunca `AppDbContext`) cumpliendo Redarbor Clean Code (<=25 líneas por método, <=1 nivel de anidación, objetos como parámetro).
- [x] `TASK-08`: Crear `CategoriesController.cs` en `Presentation/Web.API/Controllers/` decorado con `[HasPermission(Permissions.Categories.*)]` para `GET` paginado, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`.
- [x] `TASK-09`: Escribir `CategoriesIntegrationTests.cs` en `tests/Tests.IntegrationTests/` validando autenticación, PBAC (`401`/`403`), CRUD completo, paginación y rechazos `409 Conflict`.
- [x] `TASK-10`: **[TDD Green]** Verificar que `dotnet test src/backend/EnterpriseApp.sln --filter FullyQualifiedName~Categor` pase al 100% con 0 warnings.

---

## 3. Tareas de Frontend (Angular 22+ Zoneless & `@accionadev/ui-kit`)

- [x] `TASK-11`: Agregar mensajes de error `Category.*` en `src/frontend/src/app/core/config/error-messages.json` y montar `<ad-confirm-dialog />` en `app.html`.
- [x] `TASK-12`: Crear interfaces TypeScript en `src/frontend/src/app/features/categories/models/` (`category.model.ts`, `category-filter.model.ts`, `create-category-request.model.ts`, `update-category-request.model.ts`, `paged-categories-response.model.ts`).
- [x] `TASK-13`: **[TDD Red]** Escribir pruebas Jest `categories.service.spec.ts` y `categories-page.component.spec.ts` en fallo inicial.
- [x] `TASK-14`: Implementar `CategoriesService` en `src/frontend/src/app/features/categories/services/categories.service.ts`.
- [x] `TASK-15`: Implementar `CategoriesPageComponent` (`OnPush`, Signals, `<ad-table>`, `<ad-modal>`, `<ad-search-box>`, `<ad-select>`, `<ad-input>`, `<ad-textarea>`, `<ad-switch>`, `<ad-table-actions>`, `<ad-badge>`, `AdConfirmService`, `*ngxPermissionsOnly`) en `src/frontend/src/app/features/categories/pages/categories-page.component.ts`.
- [x] `TASK-16`: Registrar ruta `/categories` con `permissionGuard` (`PERMISSIONS.CATEGORIES.READ`) en `features.routes.ts`, agregar entrada en `ShellComponent.navItems()` y enlazar tarjeta en `DashboardComponent`.
- [x] `TASK-17`: **[TDD Green]** Verificar que `npm run test:ci -- -t "Categor"` pase al 100%.

---

## 4. Cierre y Quality Gate

- [x] `TASK-18`: Ejecutar verificación integral (`dotnet build --configuration Release`, `dotnet test --configuration Release` incluyendo SQL Server Docker, `npm run test:ci`, `npm run build`) y actualizar grafo con `graphify . --code-only --no-viz`.
