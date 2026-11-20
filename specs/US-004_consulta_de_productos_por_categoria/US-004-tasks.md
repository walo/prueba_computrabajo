# Desglose de Tareas Técnicas (Tasks): US-004 - Consulta de Productos por Categoría

**Issue Relacionado**: [US-004 (#8)](https://github.com/walo/prueba_computrabajo/issues/8)  
**Plan de Implementación**: [US-004-plan.md](US-004-plan.md)  

---

## 1. Tareas de Dominio & Contratos de Lectura (.NET 10)

- [x] `TASK-01`: Crear `CategoryProductsFilter.cs` y `CategoryProductsQuery.cs` en `src/backend/src/Core/Domain/ValueObjects/` (`1` record público por archivo, 100% en inglés, cumpliendo Reglas 6 y 7 de Redarbor).
- [x] `TASK-02`: Agregar `Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);` en `src/backend/src/Core/Application/Common/Interfaces/ICategoryFinder.cs` y crear `GetProductsByCategoryQuery.cs` en `src/backend/src/Core/Application/Features/Products/Queries/GetProductsByCategory/`.

---

## 2. Tareas de Aplicación CQRS & TDD (.NET 10)

- [x] `TASK-03`: **[TDD Red]** Crear `GetProductsByCategoryQueryValidatorTests.cs` y `GetProductsByCategoryQueryHandlerTests.cs` en `src/backend/tests/Tests.UnitTests/Core/Application/Features/Products/` cubriendo guardias nulas (`Request`/`Filter`), `CategoryId` vacío, paginación inválida (`Page < 1`, `PageSize` fuera de `1..100`, `Search > 150`), categoría inexistente (`Category.NotFound` sin invocar `IProductFinder.GetPagedAsync`) y propagación correcta para categoría válida (incluyendo lista vacía); verificar estado Rojo esperado.
- [x] `TASK-04`: **[TDD Green]** Implementar `GetProductsByCategoryQueryValidator.cs` y `GetProductsByCategoryQueryHandler.cs` en `src/backend/src/Core/Application/Features/Products/Queries/GetProductsByCategory/` (sin `AppDbContext` ni `IRepository`, `<= 25` líneas por método, `<= 1` nivel de anidamiento, 100% inglés); verificar que `dotnet test src/backend/EnterpriseApp.sln --configuration Release --filter FullyQualifiedName~GetProductsByCategory` pase en Verde.

---

## 3. Tareas de Infraestructura SQL Server (.NET 10)

- [x] `TASK-05`: Implementar `ExistsAsync(Guid id, CancellationToken ct = default)` en `src/backend/src/Infrastructure/Persistence/Finders/CategoryFinder.cs` usando EF Core `.AsNoTracking().AnyAsync(category => category.Id == id, ct)` (sin nueva migración ni `FOREIGN KEY`).

---

## 4. Tareas de Presentación `Web.API` & Integración (.NET 10)

- [x] `TASK-06`: Agregar el endpoint `GetProductsByCategory` (`[HttpGet("{categoryId:guid}/products")]`, `[HasPermission(Permissions.ProductsRead)]`) en `src/backend/src/Presentation/Web.API/Controllers/CategoriesController.cs` y añadir el ejemplo HTTP correspondiente en `src/backend/src/Presentation/Web.API/Web.API.http`.
- [x] `TASK-07`: **[TDD Red -> Green]** Crear `CategoryProductsIntegrationTests.cs` en `src/backend/tests/Tests.IntegrationTests/Features/Categories/` validando `401 Unauthorized`, `403 Forbidden`, `200 OK` para `ADMIN` y `USER` solo lectura con filtros (`search`, `isActive`, `lowStockOnly`, `page`, `pageSize`), `200 OK` en categoría inactiva, `200 OK` con lista vacía en categoría válida sin productos, `404 NotFound` (`Category.NotFound`) y `400 BadRequest` (`ValidationProblemDetails`); ejecutar filtro focalizado hasta alcanzar 100% Verde.

---

## 5. Cierre, Clean Code & Graphify

- [x] `TASK-08`: Ejecutar verificación global backend (`dotnet build src/backend/EnterpriseApp.sln --configuration Release` y `dotnet test src/backend/EnterpriseApp.sln --configuration Release`), auditar todos los archivos nuevos/modificados contra las **7 Reglas de Clean Code Redarbor** (`.agents/rules/redarbor-clean-code.md`) y ejecutar `graphify update .`.
