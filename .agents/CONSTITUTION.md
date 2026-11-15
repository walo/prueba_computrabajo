# Constitución de Arquitectura de Software y Gobernanza Técnica

Esta constitución define las reglas inquebrantables, los principios de diseño y los estándares de ingeniería que rigen el desarrollo de software en este repositorio. Cualquier agente autónomo, IA o desarrollador humano **debe** acatar este documento sin excepciones. El manual operativo paso a paso y el catálogo de skills orquestadoras y comandos slash se encuentran en `AGENTS.md` en la raíz del proyecto.

---

## 1. Misión y Principios Rectores

El objetivo es construir software de grado empresarial, altamente escalable, desacoplado y mantenible.
1. **Dominio Agnóstico:** Las reglas arquitectónicas son universales y aplicables a cualquier dominio de negocio (Logística, Fintech, E-commerce, SaaS o Plataformas B2B/B2C).
2. **Inmutabilidad y Reactividad:** Flujo predecible de datos, estado inmutable y reactividad declarativa sin efectos secundarios no controlados.
3. **Separación de Responsabilidades:** Respeto estricto a las fronteras entre capas. El núcleo de negocio jamás depende de la infraestructura ni del transporte.

---

## 2. Perfil Activo del Proyecto (Sellado en project-init)

> **ESTADO DE ARQUITECTURA DEL PROYECTO ACTUAL (INMUTABLE PARA TODAS LAS SKILLS):**
> 
> - **Modelo de Tenancy:**  
>   - [ ] **Multi-Tenant / B2B:** Entidades con `TenantId`, aislamiento PostgreSQL RLS por empresa y resolución vía `ITenantContext`.  
>   - [ ] **Single-Tenant / B2C (User-Centric):** Entidades pertenecientes directamente a usuarios (`UserId`). Cero `TenantId`, cero tablas de organizaciones y cero políticas RLS de empresa.
> 
> - **Plataformas Activas:**  
>   - [ ] **Backend .NET 10:** Web API RESTful con Onion Architecture concéntrica.  
>   - [ ] **Frontend Web Angular 22+:** Zoneless, Signals, Tailwind CSS y Atomic Design.  
>   - [ ] **App Mobile:** Flutter / React Native consumiendo los mismos endpoints.
> 
> ⚠️ **MANDATO DE GOBERNANZA PARA AGENTES E IA:**  
> Toda skill orquestadora posterior (`spec-generation`, `feature-scaffold`, `audit-compliance`) **DEBE** consultar este perfil antes de diseñar o auditar:  
> - Si el proyecto está marcado como **Single-Tenant**, queda **estrictamente prohibido** sugerir, exigir o implementar `TenantId`, tablas de organizaciones o políticas RLS.  
> - Si **Mobile** está desmarcado, queda **estrictamente prohibido** generar interfaces, DTOs o tareas para plataformas móviles.

---

## 3. Modelo de Tenancy y Aislamiento de Datos (Guía de Referencia)

Todo proyecto nuevo define en su fase de inicialización su modelo de pertenencia de datos:

### Modo A: Multi-Tenant / B2B (SaaS Organizacional)
- **Entidades de Tenant:** Toda entidad perteneciente a una organización implementa `ITenantEntity<TId>` (propiedad `TenantId` / `CompanyId`).
- **Aislamiento en Base de Datos (RLS):** Toda tabla con partición por tenant implementa Row Level Security (RLS) en PostgreSQL.
- **Contexto en Backend:** El tenant se resuelve en tiempo de ejecución **únicamente** desde los claims verificados del token JWT vía `ITenantContext`. Queda prohibido confiar en identificadores de tenant enviados en la URL o cuerpo de la petición.

### Modo B: Single-Tenant / User-Centric (B2C)
- **Entidades de Usuario:** Los recursos pertenecen directamente al usuario mediante `IUserOwnedEntity<TId>` (propiedad `UserId`).
- **Aislamiento Directo:** Las políticas de acceso se evalúan contra el `sub` / `userId` del JWT. Cero entidades o tablas artificiales de "empresa" u "organización" cuando el modelo de negocio no lo requiere.

---

## 4. Backend (.NET 10 & C# 13 — Onion Architecture)

```
[ Presentation: Web.API ] ──> [ Core: Application ] ──> [ Core: Domain ]
            ▲                                                 ▲
            │                                                 │
            └─────────── [ Infrastructure ] ──────────────────┘
```

### 3.1. Regla de Dependencia Concéntrica (Onion Architecture)
- **`Core/Domain` (El Centro Sagrado):** Cero dependencias externas. No referencia Entity Framework Core, ASP.NET Core, Dapper ni librerías de infraestructura. Contiene Agregados (`AggregateRoot`), Entidades (`Entity`), Value Objects inmutables, Eventos de Dominio (`IDomainEvent`), Servicios de Dominio (`DomainServices`) y contratos de Repositorio (`IRepository`).
- **`Core/Application` (Casos de Uso):** Depende únicamente de `Domain`. Organizado por **Vertical Slices / Feature Folders** (`Features/<Modulo>/<Commands|Queries>/`).
- **`Infrastructure`:** Depende de `Application` y `Domain`. Implementa persistencia (EF Core, Dapper/PostgreSQL), autenticación (JWT) y servicios externos.
- **`Presentation/Web.API`:** Punto de entrada HTTP. Depende de `Application` e `Infrastructure` para composición de DI.

### 3.2. CQRS y MediatR
- **Separación Física Estricta:** Un `Command` o `Query` y su respectivo `Handler` **jamás** residen en el mismo archivo físico.
- **Inmutabilidad:** Commands, Queries y DTOs de salida se declaran como `record` de C#.
- **Write Model:** Modifica agregados a través de interfaces `IRepository` y persiste cambios atómicos vía `IUnitOfWork`.
- **Read Model (Finders):** Las consultas de lectura (`Queries`) **prohíben** inyectar `IRepository` o hacer bucles N+1. Consumen abstracciones de búsqueda (`IFinder`) con proyecciones LINQ optimizadas (`.AsNoTracking()`, `.AsSplitQuery()`, AutoMapper a DTO).

### 3.3. Control de Errores y Validaciones
- **Prohibido el uso de Excepciones para control de flujo.** Todos los flujos de dominio o fallos esperados retornan el tipo discriminado `ErrorOr<T>`.
- **Validación Declarativa:** Cada Command/Query con entrada de datos cuenta con su validador `AbstractValidator<T>` de FluentValidation, interceptado por el pipeline global (`ValidationBehavior`).

### 3.4. Autorización y Seguridad Granular en Endpoints (RBAC Mandatorio)
- **Cero Endpoints sin Permiso:** Queda prohibido exponer endpoints protegidos únicamente con `[Authorize]`.
- **Control por Acción:** Cada acción de controlador debe exigir su permiso específico mediante políticas o atributos de autorización:
  ```csharp
  [HasPermission(Permissions.Features.Create)]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateFeatureCommand command)
  ```
- Si el token JWT carece del claim de permiso requerido, la API rechaza inmediatamente con `403 Forbidden` bajo el estándar RFC 7807 (ProblemDetails).

### 3.5. Base de Datos y Persistencia
- **Cero SQL Inline en C#:** Prohibido concatenar o incrustar consultas SQL en repositorios. Las consultas complejas o de alto volumen se encapsulan en funciones de PostgreSQL (`LANGUAGE SQL STABLE SECURITY INVOKER`).
- **Mapeos Fluidos:** Cero Data Annotations en entidades de dominio; las restricciones viven en `Persistence/Configuration/` implementando `IEntityTypeConfiguration<T>`.

### 3.6. Test-Driven Development (TDD Backend)
- Frameworks base: **xUnit**, **Moq / NSubstitute** y **FluentAssertions**.
- Estructura: Arrange, Act, Assert (AAA).
- Cobertura obligatoria: Reglas de Dominio, Handlers de Aplicación, Validadores de FluentValidation y Controladores.

---

## 5. Frontend (Angular 21+/22+ Zoneless, SDD & `@accionadev/ui-kit`)

```
../ui-design-system/ui-manifest.json        (Fase 1: Consulta del Catálogo Oficial)
                 │
                 ▼
../ui-design-system/projects/ui-kit/        (Fase 2: Átomos, Moléculas y Organismos <ad-*>)
  ├── atoms/
  ├── molecules/
  └── organisms/
                 │  (Importados vía alias '@accionadev/ui-kit')
                 ▼
src/frontend/src/app/features/<modulo>/     (Fase 3: Smart Components & Lógica de Negocio)
```

### 5.1. Filosofía SDD y Delegación al Sistema de Diseño Central (`../ui-design-system`)
- Todos los componentes visuales reutilizables (`<ad-button>`, `<ad-input>`, `<ad-card>`, `<ad-modal>`, `<ad-data-table>`, etc.) viven de forma desacoplada en el repositorio hermano **`../ui-design-system`** y se consumen mediante el alias **`@accionadev/ui-kit`**.
- **Regla de Consulta Previa:** Antes de construir cualquier vista, el agente **DEBE** inspeccionar `../ui-design-system/ui-manifest.json`.
- **Regla de Delegación Cross-Repo:** Si un control visual no existe en `ui-manifest.json` (o requiere una nueva variante), **queda terminantemente prohibido crearlo dentro del proyecto de negocio (`src/frontend/`)**. El agente debe leer y ejecutar `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` para construir el componente en `../ui-design-system/projects/ui-kit`, registrarlo en `ui-manifest.json`, publicar su página en `showcase`, y luego importarlo desde `@accionadev/ui-kit`.
- Las vistas en `features/` utilizan clases de Tailwind **únicamente** para disposición y grillas responsive (`grid`, `flex`, `gap-6`), delegando los controles visuales a `<ad-*>`.

### 5.2. Atomic Design Estricto (`@accionadev/ui-kit`)
1. **Átomo (`ad-*`):** Control UI indivisible (`ad-button`, `ad-input`, `ad-badge`, `ad-icon`, `ad-avatar`). Standalone, sin lógica de negocio, **prohibido inyectar servicios del Core o HTTP**.
2. **Molécula (`ad-*`):** Composición de 2+ átomos (`ad-form-field`, `ad-search-box`, `ad-card`, `ad-alert`). No duplica estilos de átomos internos. Cero llamadas HTTP.
3. **Organismo (`ad-*`):** Sección completa (`ad-data-table`, `ad-modal`, `ad-navbar`, `ad-drawer`). Recibe datos por entradas reactivas. Cero servicios de dominio.
4. **Feature (Smart Component en `src/frontend/src/app/features/`):** Composición de organismos y moléculas conectada a datos reales. **Único nivel autorizado** a inyectar servicios HTTP, resolver rutas y manejar peticiones de red.

### 5.3. Cero Librerías Visuales de Terceros y Parametrización por Variables CSS
- **Prohibido:** Angular Material, PrimeNG, NG-ZORRO o similares para controles visuales.
- **Permitido:** `@angular/cdk` exclusivamente para comportamientos sin opinión visual.
- El proyecto cliente hereda el preset Fluent (`presets: [require('../../../ui-design-system/tailwind.preset.js')]`) e inyecta su paleta de marca y modo oscuro mediante variables CSS (`--ad-primary`, `--ad-bg`, `--ad-surface`) en `src/frontend/src/styles.css` (prohibidos valores `#hex` arbitrarios en templates).

### 5.4. Ecosistema Reactivo de Signals (Zoneless)
- **Standalone:** Todos los componentes son standalone; sin `NgModule`.
- **Estrategia de Detección:** `ChangeDetectionStrategy.OnPush` en el 100% de los componentes.
- **Signals API:** Uso estricto de `input()`, `output()` y `model()`. Prohibidos los decoradores heredados `@Input()` y `@Output()`.
- **Data Fetching:** Se prioriza el uso declarativo de **`rxResource`** / `resource` para la adquisición de datos de la API, eliminando suscripciones manuales imperativas en `ngOnInit`.
- **Control Flow Nativo:** Exclusivamente sintaxis `@if`, `@else`, `@for (item of items(); track item.id)`, `@switch`, `@let` y vistas diferibles `@defer`. Prohibidos `*ngIf`, `*ngFor` y `*ngSwitch`.
- **Inyección de Dependencias:** Exclusivamente función `inject()`. Prohibida la inyección por constructor.
- **Formularios Reactivos:** Exclusivamente `ReactiveFormsModule` (`FormGroup`, `FormControl`). Prohibido Template-Driven forms (`[(ngModel)]`).
- **Seguridad en Plantillas:** Control de visibilidad mediante directiva declarativa de permisos `*ngxPermissionsOnly`. Prohibidas validaciones manuales de roles con `if (role === '...')`.

### 5.5. Testing Frontend
- Framework obligatorio: **Jest** (`jest-preset-angular`). Queda prohibido Karma y Jasmine.
- Cobertura: Signal Stores, Componentes OnPush y Guards funcionales.

---

## 6. Directivas Mandatorias para Agentes de IA

1. **Gobernanza de Skills Orquestadoras:** La orquestación del ciclo de vida reside exclusivamente en las 8 skills orquestadoras (`.agents/skills/<nombre>/SKILL.md`), las cuales invocan a las skills especialistas para ejecutar tareas concretas de frontend, backend y QA.
2. **Grounding en NotebookLM:** Ningún agente inventa ni asume reglas de negocio. Si una regla no está contemplada en las fuentes oficiales, es mandatorio **detenerse y preguntar al usuario**.
3. **Cero Tolerancia a Regresiones:** Ninguna fase de implementación se da por completada si existen errores de compilación o pruebas unitarias en rojo.
