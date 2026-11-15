# Desglose de Tareas Técnicas (Tasks): [KEY] - [Título de la Funcionalidad]

**Jira Issue**: [[KEY]](https://tu-empresa.atlassian.net/browse/[KEY])  
**Plan de Implementación**: [[KEY]-plan.md]([KEY]-plan.md)  

---

## 1. Tareas de Base de Datos & Dominio

- [ ] `TASK-01`: Crear entidad `[Feature]` y Value Objects en `src/Core/Domain/Entities/`.
- [ ] `TASK-02`: Definir contratos `I[Feature]Repository` e `I[Feature]DomainService` en `src/Core/Domain/`.
- [ ] `TASK-03`: Configurar mapeo EF Core en `src/Infrastructure/Persistence/Configurations/[Feature]Configuration.cs`.
- [ ] `TASK-04`: Generar y aplicar migración EF Core (`dotnet ef migrations add`).

---

## 2. Tareas de Backend (.NET 10 CQRS)

- [ ] `TASK-05`: **[TDD Red]** Escribir tests unitarios para CommandHandler y Validator en `tests/EnterpriseApp.UnitTests/`.
- [ ] `TASK-06`: Crear Command inmutable (`record`) y CommandValidator con FluentValidation.
- [ ] `TASK-07`: Implementar `[Feature]CommandHandler` inyectando `IRepository`, `IDomainService` e `IUnitOfWork`.
- [ ] `TASK-08`: Implementar Query y Finder (`I[Feature]Finder`) con `.AsNoTracking()`.
- [ ] `TASK-09`: Crear `[Feature]Controller` delgado decorado con `[HasPermission(Permissions.[Feature]...)]`.
- [ ] `TASK-10`: **[TDD Green]** Verificar que `dotnet test --filter FullyQualifiedName~[Feature]` pase al 100%.

---

## 3. Tareas de Frontend (Angular 22+ Zoneless & Tailwind)

- [ ] `TASK-11`: Auditar `src/app/shared/ui/` y construir componentes atómicos requeridos.
- [ ] `TASK-12`: Crear contratos TypeScript y servicio HTTP en `src/app/features/[feature]/services/`.
- [ ] `TASK-13`: Construir Smart Component con `ChangeDetectionStrategy.OnPush`, Signals y `rxResource`.
- [ ] `TASK-14`: Proteger acciones UI con `*ngxPermissionsOnly` y configurar rutas con `permissionGuard`.
- [ ] `TASK-15`: **[Test]** Escribir y verificar pruebas unitarias en Jest (`cmd.exe /c npm test -- -t "[Feature]"`).

---

## 4. Tareas de App Mobile (solo si la historia incluye componente Mobile)

- [ ] `TASK-M01`: Crear contratos TypeScript y servicio de consumo API para la app móvil.
- [ ] `TASK-M02`: Construir interfaz de usuario, navegación y pantallas de la funcionalidad.
- [ ] `TASK-M03`: Pruebas de integración y validación en emulador/dispositivo.

---

## 5. Cierre y Quality Gate

- [ ] `TASK-16`: Ejecutar compilación global sin warnings (`dotnet build` y `npm run build`).
- [ ] `TASK-17`: Actualizar grafo de dependencias con `graphify update .`.
