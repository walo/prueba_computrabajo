---
name: feature-scaffold
description: Full-stack TDD implementation workflow executing tasks from tasks.md in micro-cycles (Backend Red/Green -> Frontend Red/Green -> Mobile), ending with graph synchronization.
---

# Workflow: Feature Implementation (`feature-scaffold.md`)

This workflow executes the implementation of a feature following strict **Test-Driven Development (TDD)**: Red (failing test) ➔ Green (passing code) ➔ Refactor. It consumes the task checklist in `specs/<KEY>_<slug>/<KEY>-tasks.md`.

---

## 1. Invocation & Pre-Conditions

```text
/feature-scaffold [KEY]
Example: /feature-scaffold POJ-12
```

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md):
   - **Command Handlers:** Inject `IRepository`, `IDomainService`, and `IUnitOfWork`. **NEVER** inject `DbContext` or `IApplicationDbContext`.
   - **Queries:** Use `IFinder` with `.AsNoTracking()`.
   - **Domain:** Invariants protected inside AggregateRoot / Entity methods and Domain Services.
   - **Authorization:** Controller endpoints decorated with `[HasPermission(Permissions.<Feature>.<Action>)]`.
   - **Frontend:** Consume reusable `<ad-*>` components from `@accionadev/ui-kit` (`../ui-design-system/ui-manifest.json`), Smart Components with `ChangeDetectionStrategy.OnPush`, Signals, `rxResource`, and `*ngxPermissionsOnly`.
2. **Branch Pre-Condition:** The agent must be located in the dedicated feature branch `feature/<KEY>-<slug>` created during `/spec-generation`. If on `develop`, checkout the feature branch first.
3. **Spec Pre-Condition:** `specs/<KEY>_<slug>/<KEY>-tasks.md` must exist and be validated (`new-story.mjs check <KEY>`).

---

## 2. Execution Steps

### Phase 1: Database & Domain Layer
Invoke skills: **`be-dotnet-feature`** and **`be-database-sp`**
1. **Reemplazo Mandatorio del Placeholder `Features.*`:** Si el catálogo `Permissions.cs` (backend) y `permissions.constants.ts` (frontend) aún contiene el grupo temporal `Features.*`, reemplázalo obligatoriamente por el nuevo módulo de dominio (ej. `[FeatureName].Read`, `[FeatureName].Create`, `[FeatureName].Update`, `[FeatureName].Delete`) y actualiza el Seeder inicial y los roles en BD.
2. Implement Domain Entity, Value Objects, and Domain Events in `Core/Domain/Entities/`.
3. Define repository interfaces (`I[Feature]Repository`) and Domain Services (`I[Feature]DomainService`) in `Core/Domain/`.
4. Configure EF Core Fluent API mappings in `Infrastructure/Persistence/Configurations/`.
5. If heavy analytical aggregations are needed, create stored procedure/function in PostgreSQL via Dapper.
6. Create and apply database migration:
   ```bash
   dotnet ef migrations add Add[Feature]Feature
   ```

### Phase 2: Backend TDD Red Phase (Failing Tests)
Invoke skill: **`be-dotnet-test`**
1. Author unit tests for CommandHandler, Validator, and QueryHandler using xUnit, Moq, and FluentAssertions:
   - Test happy path, validation errors, domain rule violations, and not found cases.
   - Mock `IRepository` and `IUnitOfWork` (zero DbContext mocks).
2. Execute focalized test to confirm failure (**Red**):
   ```bash
   dotnet test --filter FullyQualifiedName~[Feature]
   ```
   *(Must fail because application logic is not implemented yet).*

### Phase 3: Backend Implementation Green Phase (Passing Tests)
Invoke skill: **`be-dotnet-feature`**
1. Implement Command, CommandHandler, FluentValidation validator, Query, and Finder.
2. Implement Web API Controller with `[HasPermission]`.
3. Run focalized test until all tests pass (**Green**):
   ```bash
   dotnet test --filter FullyQualifiedName~[Feature]
   ```

### Phase 4: Frontend UI Kit Verification & Cross-Repo Delegation (`@accionadev/ui-kit`)
Invoke skill: **`fe-angular-feature`** (and delegate to `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` if needed)
1. Read `../ui-design-system/ui-manifest.json` to inspect available `<ad-*>` Atoms, Molecules, and Organisms.
2. **If all required `<ad-*>` components exist in `ui-manifest.json`:** Proceed directly to Phase 5 and import them from `'@accionadev/ui-kit'`.
3. **If a required UI component does NOT exist in `ui-manifest.json` (or requires a new variant):**
   - Strictly forbidden to create ad-hoc UI components inside `src/frontend/`.
   - Read and execute `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` inside `../ui-design-system`: build the `<ad-*>` component in `projects/ui-kit`, export it in `public-api.ts`, register it in `ui-manifest.json`, and create its Showcase page.
   - Return to `src/frontend/` and consume the newly registered component via `'@accionadev/ui-kit'`.

### Phase 5: Frontend Smart Component & Signals (TDD)
Invoke skills: **`fe-angular-feature`** and **`fe-angular-test`**
1. Create TypeScript contract interfaces and HTTP service in `features/[feature]/services/`.
2. Create Smart Component in `features/[feature]/`:
   - Reactivity via Signals and `rxResource`.
   - Control Flow (`@if`, `@for (track ...)`).
   - Reactive forms with `FormGroup`.
   - Actions conditioned with `*ngxPermissionsOnly`.
3. Register navigation & PBAC visibility:
   - **Top-level feature:** Add route in `features.routes.ts` (`permissionGuard` + `data: { permissions: [PERMISSIONS.<FEATURE>.READ] }`) and **register the menu item in `ShellComponent.navItems()`** guarded by `userPerms.includes(PERMISSIONS.<FEATURE>.READ)` so it shows only when permitted.
   - **Child sub-section / submenu inside an existing feature:** Integrate it as a tab using `<ad-tabs>` inside the parent feature view (guarded by its PBAC permission), not as a nested sidebar submenu.
4. Author Jest unit tests covering loading, success, error toast, and 403 Forbidden states.
5. Run focalized test:
   ```powershell
   cmd.exe /c npm test -- -t "[Feature]"
   ```

### Phase 6: Mobile App Implementation (Conditional)
*If the story includes Mobile (`specs/<KEY>_<slug>/<KEY>-tasks.md` has `TASK-M*`):*
1. Implement Mobile screens, state store, and API service consuming the shared REST endpoints.
2. Verify in emulator or unit test suite.

### Phase 7: Knowledge Graph Update & Task Checkoff
Invoke skill: **`graphify`**
1. Mark all completed tasks with `[x]` in `specs/<KEY>_<slug>/<KEY>-tasks.md`.
2. Update the codebase knowledge graph:
   ```bash
   graphify update .
   ```

---

## 3. Quality Gate Checklist

- [ ] All tasks in `tasks.md` marked as completed (`[x]`).
- [ ] Backend Handlers inject `IRepository`, `IDomainService` and `IUnitOfWork` (0 DbContext).
- [ ] Backend tests passing 100%: `dotnet test --filter FullyQualifiedName~[Feature]`.
- [ ] Frontend tests passing 100%: `cmd.exe /c npm test -- -t "[Feature]"`.
- [ ] All controller endpoints protected with `[HasPermission]`.
- [ ] Buttons and sensitive UI actions guarded with `*ngxPermissionsOnly`.
- [ ] Knowledge graph updated via `graphify update .`.

> **Next Step:** Run `/audit-compliance <KEY>` to audit permissions and security, or `/deliver-pr` to deliver the feature via Gitflow.
