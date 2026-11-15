---
name: fe-angular-feature
description: Senior Frontend Architect skill for developing, integrating, and refactoring Angular 22+ features using Specification-Driven Development (SDD), Atomic Design, Signals, rxResource, and strict Tailwind CSS tokens. Use whenever creating or modifying UI components, features, forms, or stores in an existing Angular project.
---

# Skill: Angular Feature Development (Modern Angular + SDD + Atomic Design)

## Rol y Capacidades

Actúas como un **Senior Frontend Architect** experto en Angular moderno (v21+/v22+), TypeScript estricto, **Angular Signals**, arquitectura **Zoneless** y maquetación pura con **Tailwind CSS**.

Tu propósito es diseñar, implementar e integrar nuevas funcionalidades y componentes visuales en aplicaciones existentes siguiendo con rigor inquebrantable el manual maestro [sdd-angular-manual.md](.agents/skills/fe-angular-feature/references/sdd-angular-manual.md).

---

## 🏛️ El Principio No Negociable de UI (SDD & Delegación a `@accionadev/ui-kit`)

> **Ningún componente de UI se construye con HTML nativo ni clases de Tailwind sueltas en `features/` si existe o debe existir un átomo, molécula u organismo en `@accionadev/ui-kit` (`../ui-design-system`).**  
> 1. **Consulta Obligatoria del Manifiesto:** Antes de escribir cualquier vista, lee `../ui-design-system/ui-manifest.json`.  
> 2. **Consumo Directo:** Si el componente ya existe en el manifiesto (`<ad-button>`, `<ad-input>`, `<ad-card>`, `<ad-modal>`, etc.), impórtalo desde `'@accionadev/ui-kit'`.  
> 3. **Regla de Delegación Cross-Repo:** Si el control visual que necesitas **NO existe** en `../ui-design-system/ui-manifest.json` (o requiere una variante nueva), **queda terminantemente prohibido crearlo dentro de `src/frontend/`**. Debes pausar la vista, leer y ejecutar las instrucciones de `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` para construir el componente en `../ui-design-system/projects/ui-kit`, exportarlo en `public-api.ts`, actualizar `ui-manifest.json`, crear su página en `showcase`, y finalmente volver a tu feature para importarlo desde `'@accionadev/ui-kit'`.

---

## 🧩 Las Cuatro Capas de Atomic Design (Cross-Repo)

| Nivel | Ubicación | Responsabilidad | Inyección de Servicios |
| :--- | :--- | :--- | :--- |
| **Átomo (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/atoms/` | Control UI mínimo e indivisible (`ad-button`, `ad-input`, `ad-badge`, `ad-avatar`, `ad-spinner`). | **PROHIBIDO** inyectar servicios del Core o HTTP. Solo `input()` / `output()`. |
| **Molécula (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/molecules/` | Composición de 2+ átomos (`ad-form-field`, `ad-search-box`, `ad-card`, `ad-alert`). | **PROHIBIDO** inyectar servicios del Core o HTTP. |
| **Organismo (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/organisms/` | Sección completa de UI (`ad-data-table`, `ad-modal`, `ad-navbar`, `ad-drawer`). | Recibe datos vía `input()`. **Cero llamadas HTTP directas**. |
| **Feature (Smart Component)** | `src/frontend/src/app/features/<dominio>/` | Ensamblaje de componentes `<ad-*>` usando grillas responsive (`grid`, `flex`, `gap-6`) y lógica del cliente. | **ÚNICO nivel autorizado** a inyectar servicios HTTP, resolver rutas y consumir **`rxResource`**. |

---

## 🔄 El Flujo SDD de 5 Fases (Mandatorio)

Antes de escribir código de producción para cualquier vista de negocio, DEBES completar esta secuencia:

```
[1. Spec] ──> [2. Lectura ui-manifest.json] ──> [3. Delegación a ui-design-system (si falta)] ──> [4. Ensamblaje Smart Component] ──> [5. Validación Jest]
```

1. **Fase 1: Especificación (`specs/<KEY>_<slug>/<KEY>.spec.md`):**
   * Revisar los componentes `<ad-*>` requeridos, estados (`loading`, `empty`, `error`) y permisos PBAC (`*ngxPermissionsOnly`).
2. **Fase 2: Verificación en `../ui-design-system/ui-manifest.json`:**
   * Inspeccionar `../ui-design-system/ui-manifest.json`. Verificar que cada átomo, molécula u organismo requerido exista y cumpla con los `inputs` y `outputs` necesarios.
3. **Fase 3: Delegación al Sistema de Diseño (Solo si falta un componente):**
   * Si algún componente `<ad-*>` no existe en `ui-manifest.json`, ejecutar el flujo de `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` en el repositorio `../ui-design-system` hasta que quede registrado en `ui-manifest.json`.
4. **Fase 4: Ensamblaje del Smart Component (Standalone OnPush):**
   * Importar los componentes desde `'@accionadev/ui-kit'`.
   * Usar clases de Tailwind en la feature **únicamente** para grillas y layout responsive (`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6`, `flex`, `space-y-6`).
   * Conectar Signals (`signal()`, `computed()`, `rxResource`) y formularios reactivos (`FormGroup`).
5. **Fase 5: Validación Automatizada con Jest:**
   * Pruebas unitarias de comportamiento del Smart Component con **Jest** (`.spec.ts`).

---

## ⚡ Directrices Técnicas de Angular 21+/22+ (Signal-First)

### A. Reactividad y APIs de Signals
* **Inputs y Outputs Modernos:**
  ```typescript
  readonly label = input.required<string>();
  readonly variant = input<'primary' | 'secondary' | 'danger'>('primary');
  readonly value = model<string>(''); // Two-way binding [(value)]
  readonly submitted = output<void>(); // Event emission
  ```
* **Data Fetching Asíncrono con `rxResource`:**
  En componentes inteligentes de features (Smart Components), utiliza `rxResource` para conectar llamadas a la API de forma declarativa:
  ```typescript
  private readonly itemService = inject(ItemService);

  readonly itemsResource = rxResource({
    loader: () => this.itemService.getItems()
  });
  ```
  Esto expone automáticamente Signals: `itemsResource.value()`, `itemsResource.isLoading()` y `itemsResource.error()`.

### B. Control Flow Nativo en Plantillas
Quedan terminantemente prohibidas las directivas estructurales obsoletas (`*ngIf`, `*ngFor`, `*ngSwitch`):
```html
@if (itemsResource.isLoading()) {
  <app-spinner />
} @else if (itemsResource.error()) {
  <div class="p-4 bg-status-danger/10 text-status-danger rounded-md">
    Error al cargar elementos.
  </div>
} @else {
  @for (item of itemsResource.value()?.items ?? []; track item.id) {
    <app-item-card [item]="item" />
  } @empty {
    <p class="text-neutral-500">No hay elementos registrados en el inventario.</p>
  }
}
```

### C. Inyección de Dependencias
* **Cero Constructores:** Se prohíbe declarar dependencias en el `constructor()`.
* Utilizar siempre `inject()` a nivel de propiedad de clase:
  ```typescript
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  ```

### D. Formularios Estrictamente Reactivos
* Prohibido el uso de Template-Driven forms (`[(ngModel)]`, `ngForm`).
* Usar `ReactiveFormsModule` y definir modelos reactivos fuertemente tipados con `FormGroup` y `FormControl`.

### E. Seguridad y Control de Acceso en UI
* **Prohibido:** Validar roles manualmente en el HTML (ej. `@if (user.role === 'Admin')`).
* **Obligatorio:** Utilizar `*ngxPermissionsOnly` para mostrar/ocultar botones y acciones según permisos granulares:
  ```html
  <button 
    *ngxPermissionsOnly="['Items.Create']" 
    class="btn-primary" 
    (click)="openCreateModal()">
    Registrar Elemento
  </button>
  ```

---

## ⏳ Regla de Oro de Estados de Carga (3 Niveles UX)

Al implementar peticiones HTTP o acciones asíncronas en una feature, **nunca bloquees toda la pantalla para acciones locales**. Elige estrictamente el nivel adecuado:

1. **Nivel 1 — Acción Local / Formulario / Botón (`<ad-button [loading]="...">`):**
   - **Cuándo:** Enviar formularios (Crear, Editar, Login), confirmar un modal o ejecutar una acción puntual.
   - **Cómo:** Usar una signal local `isSubmitting = signal(false)` conectada declarativamente con el operador de utilidad `trackLoading(this.isSubmitting)` importado de `@core/utils/loading.utils` (o `finalize()`). Esto garantiza encender el loading al inicio y apagarlo siempre al terminar (éxito o error), deshabilitando solo ese botón sin congelar la navegación.
2. **Nivel 2 — Carga de Datos en Listados / Tablas (`<ad-table [loading]="...">`):**
   - **Cuándo:** Consultar paginación, filtros o recargar grillas de datos (`rxResource.isLoading()`).
   - **Cómo:** Pasar `[loading]="resource.isLoading()"` a `<ad-table>` para renderizar filas *skeleton* animadas mientras el resto del `<ad-app-shell>` sigue interactivo (cero variables booleanas manuales).
3. **Nivel 3 — Bloqueo Global de Pantalla (`GlobalLoaderComponent` / `LoaderService`):**
   - **Cuándo:** Únicamente en procesos críticos o globales donde el usuario **no debe** interactuar con nada más hasta que terminen (ej. hidratación inicial de sesión, exportaciones masivas, cierre de sesión o procesos transaccionales críticos).
   - **Cómo:** El `loaderInterceptor` es **Opt-In**; ninguna petición bloquea la pantalla por defecto. Se activa exclusivamente pasando `{ context: withGlobalLoader() }` o `{ headers: { 'x-global-loader': 'true' } }` en la petición que requiera bloqueo total.

### Iconografía y Logo de Plataforma Unificados (`<ad-icon>` y `<ad-brand-logo>`)
- **Prohibido** escribir bloques `<svg>` manuales en `features/` o `auth/`.
- Usar `<ad-icon name="..." size="sm | md | lg" />` de `@accionadev/ui-kit` para iconos del catálogo empresarial, o `<ad-icon [icon]="IconFromLucide" />` importando el icono específico desde `'lucide'` (100% *tree-shakeable*).
- Para cualquier aparición del logo/isotipo de la plataforma (Login, Onboarding, AppShell, vistas públicas), usar `<ad-brand-logo [logoUrl]="environment.logoUrl" [logoIcon]="environment.logoIcon" [alt]="environment.appName" />` (archivo `SVG` o `PNG` transparente mínimo `128×128 px` en `public/`).

### Navegación, Menú Lateral y Sub-secciones (`<ad-app-shell>`, `<ad-tabs>`, `<ad-breadcrumb>`)
- **Feature principal (primer nivel):**
  - Registrar la ruta en `features.routes.ts` protegida con `permissionGuard` y `data: { permissions: [PERMISSIONS.<MODULO>.READ] }`.
  - **Obligatorio:** Agregar el ítem correspondiente en `navItems` dentro de `ShellComponent` (`src/app/layout/shell/shell.component.ts`), envuelto en `if (userPerms.includes(PERMISSIONS.<MODULO>.READ))` para mostrarlo u ocultarlo dinámicamente según el permiso.
- **Sub-sección / Submódulo hijo dentro de una Feature (submenú):**
  - **Prohibido** anidar submenús en el sidebar; modelarlo como pestaña **`<ad-tabs>`** dentro de la vista padre, filtrando las pestañas según los permisos PBAC del usuario.
- **Sub-rutas de detalle/creación (2+ niveles)** (`/users/create`, `/orders/:id`): Usar `<ad-breadcrumb>` (o `[breadcrumbs]` en `<ad-app-shell>`) para mostrar la ruta jerárquica y permitir volver al nivel anterior.

### Patrón Canónico de Formularios (`Blueprint Oficial`)
Todo formulario de creación o edición en `features/` sigue estrictamente la estructura de `../ui-design-system` (`patterns/forms`):
1. **Elección de Contenedor por Complejidad:**
   - **1–4 campos:** `<ad-modal size="md">` con una sola columna (`space-y-4`).
   - **5–8 campos:** `<ad-modal size="lg">` con grilla de 2 columnas (`grid grid-cols-1 sm:grid-cols-2 gap-4`).
   - **> 8 campos o Maestro-Detalle:** Ruta dedicada con `<ad-breadcrumb>` + `<ad-card>` (`grid grid-cols-1 sm:grid-cols-2 gap-4`).
2. **Controles Inteligentes (`NgControl`):** Usar exclusivamente `<ad-input>`, `<ad-select>`, etc., conectados a `FormGroup` (`ReactiveFormsModule`). Los asteriscos `*` y mensajes de error reactivos son automáticos; solo usar `[errorMessages]="{ ... }"` si se requiere texto personalizado de negocio.
3. **Botones al Pie:** Alinear a la derecha con `gap-3` (`Cancelar` en `variant="outline"`, `Guardar` en `variant="primary"` y `[loading]="isSubmitting()"`).

---

## 🚫 Restricciones Críticas (Cero Tolerancias)

1. **NO** escribir código HTML nativo de botones, inputs, cards o modales dentro de `features/`. Todo control visual se importa de `@accionadev/ui-kit` (`<ad-*>`).
2. **NO** crear átomos, moléculas u organismos dentro de `src/frontend/`. Si falta un componente en `../ui-design-system/ui-manifest.json`, delégalo a `../ui-design-system/.agents/skills/fe-design-system/SKILL.md`.
3. **NO** usar librerías de UI de terceros (NG-ZORRO, Angular Material, PrimeNG).
4. **NO** omitir `ChangeDetectionStrategy.OnPush` en ningún componente.
5. **NO** utilizar `*ngIf` ni `*ngFor`.
6. **NO** emplear decoradores obsoletos `@Input()` o `@Output()`.
7. **NO** codificar ninguna vista sin haber leído previamente `../ui-design-system/ui-manifest.json`.

---

## 📁 Ejemplos de Referencia (`examples/`)

* `feature-smart-component.example.ts`: Smart Component de feature con `rxResource`, `<ad-modal>`, `<ad-button>` y notificaciones.
* `signal-store.example.ts`: Store reactivo de feature con `signal()` y `computed()`.
* `service-http.example.ts`: Servicio HTTP con DTOs y tipado estricto.
* `feature-routes.example.ts`: Definición de rutas protegidas por permisos dinámicos (PBAC).

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Especificación Aprobada** — Existe spec técnica validada antes de haber iniciado la escritura de código.
- [ ] **Manifiesto UI Verificado (`../ui-design-system/ui-manifest.json`)** — Se consultó el catálogo oficial previo al desarrollo y se importaron los controles desde `'@accionadev/ui-kit'`.
- [ ] **Desacoplamiento Estricto** — Cero creación de átomos/moléculas locales en `src/frontend/`; las vistas en `features/` solo ensamblan `<ad-*>` usando grillas responsive de Tailwind.
- [ ] **OnPush en Todos los Componentes** — `changeDetection: ChangeDetectionStrategy.OnPush` presente en todos los componentes creados o editados.
- [ ] **Signals API & Cero Decoradores** — Uso exclusivo de `input()`, `output()` y `model()`; ausencia total de `@Input()` y `@Output()`.
- [ ] **Control Flow Nativo** — Cero `*ngIf`, `*ngFor` o `*ngSwitch`; uso de `@if`, `@for (track ...)` y `@switch`.
- [ ] **Cero Subscripciones Manuales en Lectura** — Consumo asíncrono implementado con `rxResource`; estado derivado mediante `computed()`.
- [ ] **PBAC en Interfaz de Usuario** — Todo botón o acción de mutación condicionado con la directiva declarativa `*ngxPermissionsOnly="['Feature.Action']"`.
- [ ] **Menú Lateral / Pestañas PBAC** — Feature principal registrada en `features.routes.ts` (`permissionGuard`) y agregada a `ShellComponent.navItems()` condicionada por `PERMISSIONS.<MODULO>.READ` (o integrada con `<ad-tabs>` si es sub-sección hija).
- [ ] **Formularios Fuertemente Tipados** — Uso de `ReactiveFormsModule` con validación declarativa y mapeo semántico de errores.
- [ ] **Pruebas Unitarias Focalizadas en Verde** — Ejecución exitosa de `cmd.exe /c npm test -- -t "<FeatureName>"`.

