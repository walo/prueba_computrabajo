# sdd-angular-manual.md — Manual de Desarrollo Frontend (Angular 22+ & `@accionadev/ui-kit`)

> Este documento es la fuente de verdad técnica y metodológica para cualquier desarrollador o
> agente de IA que trabaje en el frontend Angular (v21+/v22+). Complementa
> `CONSTITUTION.md` y regula el consumo y delegación hacia el Sistema de Diseño
> centralizado en `../ui-design-system` (`@accionadev/ui-kit`).

---

## 1. Filosofía: Specification-Driven Development (SDD) & UI Kit Centralizado

**El principio central de este manual, no negociable:**

> Ninguna vista de negocio en `features/` se construye con HTML nativo ni clases de Tailwind sueltas
> para botones, inputs, cards, tablas o modales. **Todos los controles visuales viven en el repositorio hermano
> `../ui-design-system` y se importan desde `'@accionadev/ui-kit'` usando el prefijo `<ad-*>`**.
> Antes de construir cualquier pantalla, es obligatorio leer `../ui-design-system/ui-manifest.json`.
> Si el componente `<ad-*>` que necesitas no existe en el manifiesto, se delega su creación siguiendo
> `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` — nunca se crea un componente UI local dentro de `src/frontend/`.

---

## 2. Atomic Design Cross-Repo: Los Cuatro Niveles

| Nivel | Ubicación | Ejemplos (`@accionadev/ui-kit`) | Regla |
| :--- | :--- | :--- | :--- |
| **Átomo (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/atoms/` | `ad-button`, `ad-input`, `ad-badge`, `ad-icon`, `ad-avatar`, `ad-spinner`, `ad-checkbox`, `ad-switch` | Standalone, OnPush, sin lógica de negocio, sin llamadas a servicios. Solo `input()` / `output()`. |
| **Molécula (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/molecules/` | `ad-form-field`, `ad-search-box`, `ad-card`, `ad-alert`, `ad-stat-card` | Compone átomos sin reimplementar sus estilos. Cero llamadas HTTP. |
| **Organismo (`<ad-*>`)** | `../ui-design-system/projects/ui-kit/src/lib/organisms/` | `ad-data-table`, `ad-modal`, `ad-navbar`, `ad-drawer` | Recibe datos por `input()` y emite eventos por `output()`. Cero servicios de dominio. |
| **Feature (Smart Component)** | `src/frontend/src/app/features/<modulo>/` | `FeatureListComponent`, `DashboardComponent` | Único nivel que inyecta servicios HTTP, resuelve rutas, aplica `*ngxPermissionsOnly` y orquesta estado con `rxResource` y Signals. |

---

## 3. Tailwind CSS y Tokens Semánticos (`--ad-*`)

1. **Preset Compartido:** `src/frontend/tailwind.config.js` hereda `presets: [require('../../../ui-design-system/tailwind.preset.js')]` y escanea `../../../ui-design-system/projects/ui-kit/src/**/*.{html,ts}`.
2. **Colores por Variables CSS del Cliente:** En `src/frontend/src/styles.css` se definen los valores RGB de `--ad-primary`, `--ad-bg`, `--ad-surface`, `--ad-text`, `--ad-border` y el bloque `.dark`. Prohibido usar valores `#hex` arbitrarios en templates.
3. **Frontera de Grillas vs Controles:** Las vistas en `features/` usan Tailwind exclusivamente para el layout y grillas responsive (`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6`, `flex`, `space-y-6`), mientras que los controles interactivos son siempre `<ad-*>`.

---

## 4. Convenciones de Componente Angular 22+

- **Standalone Components** en toda la aplicación — cero `NgModule`.
- **Angular Signals** para estado reactivo (`signal()`, `computed()`, `input()`, `output()`, `model()`).
- **Data Fetching Declarativo con `rxResource`:** en Smart Components, evitando suscripciones manuales en queries de lectura.
- **`ChangeDetectionStrategy.OnPush`** en el 100% de los componentes.
- **Control Flow Nativo:** `@if`, `@else`, `@for (item of items(); track item.id)`, `@switch`. Prohibidos `*ngIf` y `*ngFor`.
- **Inyección de Dependencias:** exclusivamente con `inject()`.
