---
name: fe-angular-scaffold
description: Principal Frontend Architect skill for scaffolding and initializing standalone Angular 22+ Zoneless projects from scratch with Jest, Tailwind CSS v3 semantic tokens, provideAppInitializer dynamic PBAC hydration (/auth/me), Silent Refresh Token Rotation interceptor, separated auth/ vs features/ routing, and transversal 403/404 StatusPageComponent. Use exclusively when creating a new Angular application from zero.
---


# Skill: Angular Scaffold Skeleton (Angular 22+ Zoneless + Dynamic PBAC + Jest)

## Rol y Capacidades

Actúas como un **Principal Frontend Architect** experto en **Angular 22+**, arquitectura **Zoneless** (`provideZonelessChangeDetection()`), **Signals**, **Tailwind CSS v3** con tokens semánticos, testing con **Jest** (`jest-preset-angular`) y autorización granular **PBAC** con `ngx-permissions`.

Tu misión es entregar un cascarón frontend empresarial **100% funcional desde el minuto cero**, con ciclo de sesión completo (`login`, hidratación en `F5` vía `provideAppInitializer` + `GET /api/v1/auth/me`, Silent Refresh Token Rotation en `tokenInterceptor`, `logout` con invalidación en backend), separación limpia entre `src/app/auth/` y `src/app/features/`, y página de estado transversal (`StatusPageComponent`) para `/forbidden` (`403`) y rutas inexistentes `/not-found` / `**` (`404`).

---


## 🏛️ Pilares Inquebrantables del Cascarón Frontend

3. **Zoneless Puro (`provideZonelessChangeDetection()`):** Prohibido `zone.js` o `provideZoneChangeDetection()`. Todos los componentes usan `ChangeDetectionStrategy.OnPush`, `inject()` y Signals (`signal`, `computed`, `input`, `output`).
4. **PBAC Dinámico sin Permisos en JWT ni `localStorage`:**
   - El JWT es ultraligero y **no** transporta el arreglo de permisos.
   - Queda **terminantemente prohibido** crear `roles.enum.ts` o guardar permisos en `localStorage`.
   - En `app.config.ts`, `provideAppInitializer(() => inject(AuthService).initializeSession())` consulta `GET /api/v1/auth/me` **antes** de que el Router ejecute `authGuard` o `permissionGuard`, cargando los permisos en memoria con `NgxPermissionsService.loadPermissions()`. Así la recarga de navegador (`F5`) funciona sin falsos bloqueos.
5. **Silent Refresh Token Rotation (`tokenInterceptor`):**
   - Ante un `401 Unauthorized` (fuera de `/auth/login`, `/auth/refresh` y `/auth/logout`), encola peticiones concurrentes con un `BehaviorSubject`, invoca `POST /api/v1/auth/refresh`, actualiza el par de tokens en `StorageService` y reintenta la petición original de forma transparente. Solo cierra sesión si el refresh falla.
6. **Separación `auth/` vs `features/` + Conexión a `@accionadev/ui-kit` (Copia Literal Obligatoria):**
   - **Regla de Copia Fiel:** Copia literalmente las plantillas de `examples/cascaron/` (`styles.example.css`, `environment.example.ts`, `login.component.example.ts`, `shell.component.example.ts`, `dashboard.component.example.ts`, `toast-container.component.example.ts`, `loader.component.example.ts`, `status-page.component.example.ts`) sin reescribirlas desde cero ni omitir variables `--ad-*` o `<ad-brand-logo>`. Para personalizar un proyecto nuevo, ajusta únicamente los valores en `environment.ts` (`appName`, `appSubtitle`, `logoUrl`, `logoIcon`, `companyName`) y, si el cliente pide otro color de marca, cambia solo los canales RGB de `--ad-primary*` y `--ad-ring` en `styles.css`.
   - `src/app/auth/` (`auth.routes.ts` + `login.component.ts` protegido por `publicGuard`): Basado en `examples/cascaron/login.component.example.ts`, consumiendo `<ad-card>`, `<ad-brand-logo>` (conectado a `environment.logoUrl` y `environment.logoIcon`), `<ad-alert>`, `<ad-input>` (con asterisco `*` y validación reactiva automática vía `NgControl`) y `<ad-button [fullWidth]="true">`. Al compilar y abrir la app sin sesión, aterriza inmediatamente en `/auth/login`.
   - `src/app/layout/shell/shell.component.ts`: Basado en `examples/cascaron/shell.component.example.ts`, montando `<ad-app-shell>` de `@accionadev/ui-kit` conectado a `environment` (`appName`, `appSubtitle`, `logoUrl`, `logoIcon`, `companyName`, `version`), Sidebar colapsable con permisos PBAC, Topbar con Dropdown de Perfil (*Mi Perfil*, *Configuración*, *Modo Oscuro*, *Cerrar sesión*), `<router-outlet />` y Footer.
   - **Identidad de Plataforma Única (`environment.ts` + `<ad-brand-logo>`):** Todos los puntos donde aparece el logo/isotipo de la plataforma (Login, AppShell y vistas públicas) leen `environment.logoUrl` (archivo `SVG` o `PNG` transparente mínimo `128x128px` en `public/`) y usan `environment.logoIcon` como fallback uniforme con `<ad-icon>`.
   - `src/app/features/` (`features.routes.ts` + `dashboard.component.ts` protegido por `authGuard` y `permissionGuard`): Basado en `examples/cascaron/dashboard.component.example.ts`, mostrando fila de 3 tarjetas KPI con `<ad-card>` + `<ad-badge>` y matriz de permisos PBAC con validación `*ngxPermissionsOnly`.
   - `src/app/shared/components/toast-container.component.ts`: Basado en `examples/cascaron/toast-container.component.example.ts`, ubicado en la esquina superior derecha (`fixed top-20 right-6 z-50`) renderizando `<ad-alert>` de `@accionadev/ui-kit`.
7. **Componente Transversal `StatusPageComponent` (`403` y `404`):**
   - Ubicado en `src/app/shared/pages/status-page/status-page.component.ts`.
   - Reutilizado por `/forbidden` (`code: '403'`, cuando `permissionGuard` rechaza el acceso a una ruta sin permisos) y por `/not-found` + wildcard `**` (`code: '404'`, cuando el usuario ingresa una URL que no existe), ofreciendo retorno inteligente a `/dashboard` (si hay sesión) o `/auth/login` (si no hay sesión).

---


## 🛠️ Flujo de Comandos CLI para Scaffolding

### Paso 1: Creación del Proyecto Standalone Zoneless

```bash
npx -y @angular/cli@latest new frontend --standalone --routing --style=css --ssr=false --skip-tests
cd frontend
```


### Paso 2: Configurar Jest Zoneless (`jest-preset-angular` v17+)

```bash
npm install -D jest jest-preset-angular @types/jest jest-environment-jsdom jsdom ts-node --legacy-peer-deps
```

- `setup-jest.ts`:
  ```typescript
  import { setupZonelessTestEnv } from "jest-preset-angular/setup-env/zoneless";
  setupZonelessTestEnv();
  ```
- `jest.config.js`: Usar `createCjsPreset({ tsconfig: '<rootDir>/tsconfig.spec.json' })`, `setupFilesAfterEnv: ['<rootDir>/setup-jest.ts']` y mapear el alias del Sistema de Diseño + instancia única de `@angular/*`:
  ```javascript
  moduleNameMapper: {
    '^@accionadev/ui-kit$': '<rootDir>/../../../ui-design-system/projects/ui-kit/src/public-api.ts',
    '^@angular/core$': require.resolve('@angular/core'),
    '^@angular/common$': require.resolve('@angular/common'),
    '^@angular/forms$': require.resolve('@angular/forms'),
    '^@angular/router$': require.resolve('@angular/router'),
  }
  ```
- `package.json`: `"start": "ng serve --prebundle=false"` (evita que Vite pre-empaquete una segunda instancia de `@angular/core` al importar desde `../ui-design-system` y cause `NG0203`), `"test": "jest"`, `"test:ci": "jest --ci --runInBand"`.


### Paso 3: Conectar `@accionadev/ui-kit` (Rutas Relativas), Tailwind Preset & `ngx-permissions`

```bash
npm install ngx-permissions lucide --legacy-peer-deps
npm install -D tailwindcss@^3.4.17 postcss@^8.4.49 autoprefixer@^10.4.20 --legacy-peer-deps
```

- En `tsconfig.json`, registrar el path alias relativo hacia el repositorio hermano `ui-design-system` y forzar instancia única de `@angular/*` y `rxjs` (sin `baseUrl` deprecado en TS 6) para evitar `NG0203`:
  ```json
  "paths": {
    "@accionadev/ui-kit": ["./../../../ui-design-system/projects/ui-kit/src/public-api.ts"],
    "@angular/*": ["./node_modules/@angular/*"],
    "rxjs": ["./node_modules/rxjs"],
    "rxjs/*": ["./node_modules/rxjs/*"]
  }
  ```
- Crear `postcss.config.js` con `tailwindcss: {}` y `autoprefixer: {}`.
- Configurar `tailwind.config.js` heredando el preset Fluent de `ui-design-system` y escaneando sus componentes:
  ```javascript
  module.exports = {
    presets: [require('../../../ui-design-system/tailwind.preset.js')],
    content: [
      './src/**/*.{html,ts}',
      '../../../ui-design-system/projects/ui-kit/src/**/*.{html,ts}',
    ],
  };
  ```
- En `src/styles.css`, copiar la plantilla base `examples/cascaron/styles.example.css` que incluye `@tailwind base; @tailwind components; @tailwind utilities;` y define los tokens CSS de marca por defecto en **Azul Corporativo** (`--ad-primary: 0 82 154` / `#00529A`, `--ad-bg: 247 249 252` / `#F7F9FC`, estados semánticos y su contraparte `.dark` en `#039DE9` / `3 157 233`). Prohibido usar valores `#hex` arbitrarios en plantillas HTML.


### Paso 4: Estructura Canónica de Directorios (`src/`)

```text
src/
├── environments/
│   ├── environment.ts                       # apiUrl: 'http://localhost:5000/api/v1'
│   └── environment.prod.ts                  # apiUrl: '/api/v1'
│
└── app/
    ├── core/
    │   ├── constants/
    │   │   ├── api-paths.constants.ts       # /auth/login, /auth/refresh, /auth/logout, /auth/me
    │   │   └── permissions.constants.ts     # Espejo exacto de Permissions.cs del backend
    │   ├── guards/
    │   │   ├── auth.guard.ts                # Redirige a /auth/login si no hay sesión
    │   │   ├── public.guard.ts              # Redirige a /dashboard si ya hay sesión
    │   │   └── permission.guard.ts          # Valida route.data['permissions'] -> /forbidden
    │   ├── interceptors/
    │   │   ├── loader.interceptor.ts        # Interceptor Opt-In para bloqueo total Nivel 3 (x-global-loader / USE_GLOBAL_LOADER)
    │   │   └── token.interceptor.ts         # Bearer JWT + Silent Refresh Queue en 401
    │   ├── models/
    │   │   ├── auth.models.ts               # LoginRequest, AuthTokensResponse, CurrentUser
    │   │   └── problem-details.model.ts     # Estándar RFC 7807
    │   ├── utils/
    │   │   └── loading.utils.ts             # trackLoading (Nivel 1 para botones) y withGlobalLoader (Nivel 3)
    │   └── services/
    │       ├── storage.service.ts           # SSR-safe wrapper solo para access/refresh tokens
    │       ├── auth.service.ts              # login, refreshSession, initializeSession (/auth/me), logout
    │       ├── loader.service.ts            # Signal isLoading
    │       └── notification.service.ts      # Signal notifications + extractor ProblemDetails
    │
    ├── shared/
    │   └── pages/
    │       └── status-page/                 # Transversal 403 (/forbidden) y 404 (/not-found, **)
    │                                        # (Nota: Átomos, moléculas y organismos viven en @accionadev/ui-kit)
    │
    ├── layout/
    │   └── shell/                           # ShellComponent autenticado con <router-outlet />
    │
    ├── auth/                                # Dominio público de autenticación (fuera de features/)
    │   ├── auth.routes.ts
    │   └── login/                           # LoginComponent base funcional mínimo
    │
    ├── features/                            # Módulos privados de negocio
    │   ├── features.routes.ts
    │   └── dashboard/                       # DashboardComponent base funcional mínimo
    │
    ├── app.routes.ts
    ├── app.config.ts                        # provideZonelessChangeDetection + provideAppInitializer
    └── app.ts
````

---

## 📁 Catálogo de Plantillas (`examples/cascaron/`)

- `app-config.example.ts`: Configuración Zoneless + `provideAppInitializer` + `NgxPermissionsModule.forRoot()`.
- `app-routes.example.ts`: Rutas raíz (`auth` con `publicGuard`, `forbidden` 403, `not-found` 404, `ShellComponent` con `authGuard` -> `features.routes`, y wildcard `**`).
- `features-routes.example.ts`: Rutas privadas de negocio protegidas con `permissionGuard`.
- `auth.service.example.ts`: Ciclo completo de sesión (`login` -> `/auth/me`, `refreshSession`, `initializeSession`, `logout`).
- `token.interceptor.example.ts`: Adjunta Bearer y ejecuta Silent Refresh Token Rotation ante `401`.
- `guards.example.ts`: `authGuard`, `publicGuard` y `permissionGuard` (redirige a `/forbidden`).
- `status-page.component.example.ts`: Componente transversal para `403` (`/forbidden`) y `404` (`/not-found` / `**`).
- `shell.component.example.ts`: Layout privado autenticado con `<ad-app-shell>` (Sidebar colapsable, Topbar con Dropdown de Perfil + Modo Oscuro y Footer).
- `login.component.example.ts`: Login corporativo con `<ad-card>`, `<ad-alert>`, `<ad-input>` (validación reactiva automática) y `<ad-button>`.
- `dashboard.component.example.ts`: Dashboard ejecutivo inicial con tarjetas KPI `<ad-card>` + `<ad-badge>` y matriz PBAC.
- `toast-container.component.example.ts`: Contenedor de notificaciones flotantes arriba a la derecha (`top-20 right-6`) con `<ad-alert>`.
- `styles.example.css`: Hoja de estilos global con la paleta base por defecto en Azul Corporativo (`#00529A` -> `0 82 154`), variables `--ad-*` y soporte nativo `.dark`.
- `storage.service.example.ts`, `loader.service.example.ts`, `loader.interceptor.example.ts`, `loading.utils.example.ts`, `notification.service.example.ts`, `permissions.constants.example.ts`, `environment.example.ts`, `tailwind.config.example.js`.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate)

- [ ] **Zoneless & OnPush 100%** — `provideZonelessChangeDetection()` activo; `ChangeDetectionStrategy.OnPush` e `inject()` en todos los componentes.
- [ ] **Rehidratación F5 vía `provideAppInitializer`** — Permisos cargados desde `GET /api/v1/auth/me` antes de activar rutas; cero permisos en `localStorage` y cero `roles.enum.ts`.
- [ ] **Silent Refresh Operativo** — `tokenInterceptor` renueva sesión con `POST /api/v1/auth/refresh` ante `401` antes de forzar logout.
- [ ] **Separación `auth/` vs `features/` + `StatusPageComponent`** — `/auth/login` público, `/dashboard` privado, `/forbidden` (403) y `**` -> `/not-found` (404) resueltos por el componente transversal.
- [ ] **Jest en Verde + Build Limpio** — `npm run test:ci` y `npm run build` ejecutan con 0 errores.
