# Especificación Técnica: [KEY] - [Título de la Funcionalidad]

**Jira Issue**: [[KEY]](https://tu-empresa.atlassian.net/browse/[KEY])  
**Estado Jira**: En Curso (In Progress)  
**Entrevista Técnica**: [[KEY]-interview.md]([KEY]-interview.md)  
**Fecha**: [AAAA-MM-DD]  

---

## 1. Contratos de Datos y API (Interface Definition)

### A. DTOs de Entrada (Request Models)
```typescript
// Contrato cliente TypeScript
export interface Create[Feature]Request {
  code: string;
  name: string;
  description: string;
}
```

```csharp
// Contrato backend C# (inmutable)
public sealed record Create[Feature]Command(
    Guid CompanyId,
    string Code,
    string Name,
    string Description) : IRequest<ErrorOr<Guid>>;
```

### B. DTOs de Salida (Response Models)
```typescript
export interface [Feature]Dto {
  id: string;
  code: string;
  name: string;
  description: string;
  isActive: boolean;
}
```

### C. Endpoints HTTP (RESTful RFC 7807)
| Método | Ruta | Permiso Requerido (PBAC) | Éxito | Errores Posibles |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/[features]` | `[Features.Create]` | `201 Created` | `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `409 Conflict` |
| `GET` | `/api/v1/[features]` | `[Features.Read]` | `200 OK` | `401 Unauthorized`, `403 Forbidden` |
| `GET` | `/api/v1/[features]/{id}` | `[Features.Read]` | `200 OK` | `401 Unauthorized`, `403 Forbidden`, `404 NotFound` |

---

## 2. Diseño de Backend (.NET 10 Onion Architecture)

- **Dominio (`Core/Domain`):**
  - Entidad / Agregado: `[Feature]` con métodos de fábrica `Create(...)` y eventos `[Feature]CreatedDomainEvent`.
  - Repositorio: `I[Feature]Repository` en `Domain/Repositories/`.
  - Domain Service: `I[Feature]DomainService` para validaciones cruzadas.
  - Unit of Work: `IUnitOfWork` para persistencia.
- **Aplicación (`Core/Application`):**
  - Commands: `Create[Feature]Command`, `Create[Feature]CommandHandler`, `Create[Feature]CommandValidator`.
  - Queries: `Get[Feature]sQuery`, `Get[Feature]sQueryHandler`, `I[Feature]Finder` (`.AsNoTracking()`).
- **Pruebas Unitarias (TDD):**
  - Handlers con mocks de repositorios y `IUnitOfWork`.
  - Validadores con `TestValidate()`.

---

## 3. Diseño de Frontend (Angular 22+ Zoneless & Tailwind)

- **Atomic Design (`shared/ui/`):**
  - Componentes reutilizables existentes: [Button, Input, ModalShell, etc.]
  - Componentes nuevos a crear primero en `shared/ui/`: [Átomos, moléculas u organismos requeridos]
  - Cero librerías de terceros (100% Tailwind CSS y tokens de diseño corporativos).
- **Smart Component (`features/[feature]/`):**
  - Consumo reactivo con `rxResource` y Signals.
  - `changeDetection: ChangeDetectionStrategy.OnPush`.
  - Control Flow nativo (`@if`, `@for (track ...)`).
  - Formularios reactivos puros (`FormGroup`).
  - Botones condicionados con `*ngxPermissionsOnly`.

---

## 4. Diseño de App Mobile (solo si la historia incluye componente Mobile)

- **Pantallas y Navegación:** [Rutas, componentes visuales y flujo móvil]
- **Consumo de API:** Reutiliza los mismos DTOs y endpoints definidos en la Sección 1.
- **Interacciones Táctiles y Estados:** [Manejo de estado, feedback táctil y modo offline/online]

---

## 5. Catálogo de Errores Semánticos (UI Error Translation)

| Código de Dominio (`code`) | HTTP Status | Mensaje en UI para el Usuario |
| :--- | :--- | :--- |
| `[Feature].NotFound` | `404` | "El registro solicitado no existe o fue dado de baja." |
| `[Feature].DuplicateCode` | `409` | "Ya existe un elemento registrado con este código." |
| `Company.NotFound` | `404` | "La empresa asociada no existe o se encuentra inactiva." |
