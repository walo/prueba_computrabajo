---
trigger: always_on
---

# Regla: Control de Acceso Granular por Permisos Dinámicos (PBAC / RBAC Dinámico)

## 1. Principio Fundamental: Permisos como Fuente Única de Autorización

Para evitar despliegues de frontend y recompilaciones cada vez que se crea un rol nuevo en la base de datos, **el sistema valida PERMISOS tanto en navegación de pantallas como en ejecución de acciones**:

```
                       USUARIO AUTENTICADO
                                │
        ┌───────────────────────┴───────────────────────┐
        ▼                                               ▼
  RUTAS / PANTALLAS (Macro)                   ACCIONES / BOTONES (Micro)
 "¿Tengo permiso de ver este módulo?"         "¿Tengo permiso de pulsar este botón?"
        │                                               │
  Rutas Frontend (Angular)                    Botones / Acciones (Angular)
  Guard: `permissionGuard`                    Directiva: `*ngxPermissionsOnly`
  Requiere: `['Features.Read']`               Requiere: `['Features.Create']`
  Sin permiso: Redirige a `/forbidden` (403)  Resultado: Oculto si carece del permiso
                                                        │
                                                        ▼
                                              Backend .NET (C#)
                                              Atributo: `[HasPermission(...)]`
                                              Resultado: 403 Forbidden si se fuerza por API
```

### ¿Por qué NO usar Roles fijos en el código frontend?

1. **Flexibilidad Dinámica:** Los Roles en la base de datos son agrupadores de permisos. Si un administrador crea el rol _"Auditor Regional"_ en BD y le asigna el permiso `Features.Read`, el usuario accede inmediatamente a la pantalla sin tocar una sola línea de código TypeScript.
3. **Cero Recompilaciones:** Los permisos son constantes inmutables del dominio; los roles son configurables por el cliente/empresa en tiempo de ejecución. Queda prohibido crear `roles.enum.ts` en el frontend.

---


## 2. Implementación en Frontend (Angular 22+ Zoneless)


### A. Hidratación de Sesión y Permisos en Arranque / F5 (`provideAppInitializer`)

- El JWT es **ultraligero** (no contiene el arreglo de permisos) y los permisos **jamás** se persisten en `localStorage`.
- En `app.config.ts`, `provideAppInitializer(() => inject(AuthService).initializeSession())` consulta `GET /api/v1/auth/me` antes de activar el Router e hidrata `NgxPermissionsService.loadPermissions(user.permissions)`. Así `permissionGuard` cuenta con los permisos en memoria incluso tras presionar `F5`.

### B. Control de Rutas, Menú Lateral y Pestañas por Permiso (`features/*.routes.ts` + `ShellComponent` + `<ad-tabs>`)

- **Rutas (`features/*.routes.ts`):** Toda feature privada de negocio valida su **Permiso** mediante `permissionGuard`, el cual redirige a `/forbidden` (`StatusPageComponent` transversal `403` / `404`) si falta el permiso:

```typescript
export const FEATURE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [permissionGuard],
    data: { permissions: ['Features.Read'] },
    loadComponent: () => import('./components/feature-list.component').then(m => m.FeatureListComponent)
  }
];
```

- **Menú Lateral Obligatorio (`ShellComponent.navItems()`):** Toda **Feature principal nueva** debe registrarse obligatoriamente en `navItems` de `ShellComponent` (`src/app/layout/shell/shell.component.ts`), condicionada por su permiso de lectura para que se muestre únicamente si el usuario posee el permiso y permanezca oculta en caso contrario:

```typescript
if (userPerms.includes(PERMISSIONS.USERS.READ)) {
  items.push({
    id: 'users',
    label: 'Usuarios',
    route: '/users',
    icon: 'users',
    active: url.startsWith('/users'),
  });
}
```

- **Submenús / Sub-secciones Hijas (`<ad-tabs>`):** Si la funcionalidad es una **sub-sección o submódulo hijo** dentro de una feature existente, **NO** se crean submenús anidados en el sidebar: se integra como pestaña dentro de la vista padre usando **`<ad-tabs>`** de `@accionadev/ui-kit`, condicionando igualmente su visibilidad al permiso PBAC correspondiente.


### C. Control de Visibilidad en Plantillas por Permiso de Acción

- **Obligatorio:** Condicionar botones, formularios y acciones con la directiva declarativa `*ngxPermissionsOnly`:

```html
<!-- Solo visible si el usuario posee el permiso granular de creación -->
<button
  *ngxPermissionsOnly="['Features.Create']"
  class="btn-primary"
  (click)="openCreateModal()">
  Crear Registro
</button>

<!-- Solo visible si el usuario posee el permiso granular de eliminación -->
<button
  *ngxPermissionsOnly="['Features.Delete']"
  class="btn-danger"
  (click)="deleteItem(item.id)">
  Eliminar
</button>
```

---

## 3. Implementación en Backend (.NET 10 / C# 13)

### A. Catálogo Centralizado de Permisos

Declarado como constantes estáticas en `Core/Domain/Constants/Permissions.cs`:

```csharp
public static class Permissions
{
    // Módulos transversales de Cascarón Base
    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    // Nota de Dominio: El grupo 'Features.*' en el cascarón inicial es un placeholder temporal.
    // Al implementar la primera Feature de negocio (ej. Devices, Billing, Inventory),
    // el agente DEBE reemplazar 'Features.*' por los permisos reales del dominio correspondiente.
    public static class Features
    {
        public const string Read = "Features.Read";
        public const string Create = "Features.Create";
        public const string Update = "Features.Update";
        public const string Delete = "Features.Delete";
    }
}
```

### B. Obligatoriedad por Endpoint en Controladores

- **Endpoints de negocio/administración:** **Obligatorio** decorar cada acción con `[HasPermission(...)]`. Prohibido usar `[Authorize]` genérico en controladores de dominio.
- **Única excepción transversal de sesión propia (`AuthController`):**
  - Públicos (`[AllowAnonymous]`): `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `GET /health`, `GET /health/ready`.
  - Sesión propia (`[Authorize]`): `GET /api/v1/auth/me` (retorna identidad + `permissions` propios desde `IPermissionCacheService`) y `POST /api/v1/auth/logout` (invalida `jti` en Blacklist O(1) y revoca Refresh Tokens). Todo usuario autenticado —incluso sin permisos administrativos como `Users.Read`— debe poder consultar su propia sesión y cerrarla.

```csharp
[Route("api/v1/features")]
public sealed class FeaturesController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Features.Read)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ...

    [HttpPost]
    [HasPermission(Permissions.Features.Create)]
    public async Task<IActionResult> Create([FromBody] CreateFeatureCommand cmd, CancellationToken ct) => ...

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Features.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => ...
}
```


### C. Rechazo Estricto (403 Forbidden vs 401 Unauthorized)

- Si el token fue revocado en `JwtBlacklistMiddleware`, retorna **`401 Unauthorized`** (RFC 7807 ProblemDetails).
- Si el usuario está autenticado pero carece del permiso requerido en `PermissionAuthorizationMiddleware`:
  1. Se intercepta antes de llegar al Controller/Handler consultando `IPermissionCacheService`.
  2. Retorna **`403 Forbidden`** bajo RFC 7807 (`ProblemDetails`).
  3. Queda prohibido devolver `401 Unauthorized` si el token es válido pero solo carece del permiso.

---

## 4. Presencia Obligatoria en Cascarón y Features

| Capa         | Cascarón Base (`scaffold-skeleton`)                                                                                                                                                                                                                                                         | Nuevas Features (`feature-development`)                                                                                                    |
| :----------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------------------------------------------------------------------------------------------------------------------------------------- |
| **Frontend** | Genera `authGuard`, `publicGuard`, `permissionGuard`, `StatusPageComponent` (`/forbidden` 403 y `/not-found` 404), catálogo `permissions.constants.ts`, Silent Refresh en `tokenInterceptor`, `NgxPermissionsModule.forRoot()` y `provideAppInitializer` consumiendo `GET /api/v1/auth/me`. | Define `data: { permissions: [...] }` en `features.routes.ts`, registra obligatoriamente el ítem en `ShellComponent.navItems()` filtrado por permiso (o usa `<ad-tabs>` si es sub-sección hija) y decora acciones con `*ngxPermissionsOnly`. |
| **Backend**  | Genera `[HasPermission]`, `JwtBlacklistMiddleware`, `PermissionAuthorizationMiddleware`, `IPermissionCacheService`, endpoints base (`login`, `refresh`, `logout`, `me`, `/health`) y respuestas RFC 7807.                                                                                   | Agrega constantes a `Permissions.cs` y decora cada endpoint del controlador con `[HasPermission]`.                                         |
