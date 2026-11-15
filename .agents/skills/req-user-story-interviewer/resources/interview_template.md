# Entrevista Técnica y de Negocio: [Título de la Historia]

**Historia ID / Jira Issue**: [KEY]  
**Jira URL**: https://[tu-empresa].atlassian.net/browse/[KEY]  
**Fecha de la Sesión**: [AAAA-MM-DD]  
**Entrevistador**: [Agente IA / Nombre del Analista]  
**Entrevistado (Stakeholder/PO/Dev)**: [Nombre o Rol]  
**Estado Jira**: [En Curso | In Progress]  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada y gestionada en Jira Cloud  

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
[Explicar con claridad el valor de negocio de esta funcionalidad y cómo opera dentro de la jerarquía o dominio de la aplicación (ej. Tenant → Organización / Sede → Recursos, o según lo definido en NotebookLM). ¿Qué dolor resuelve y quién se beneficia directamente?]

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - [Característica o flujo 1 acordado]
  - [Característica o flujo 2 acordado]
- **Fuera del alcance explícito (Out-of-Scope)**:
  - [Característica expresamente postergada o descartada para esta historia]

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

> Cada bloque representa un área de indagación clave resuelta durante la entrevista técnica.

### Bloque A: Dominio y Reglas de Negocio

#### Pregunta A.1: [Enunciado de la pregunta de negocio/dominio]
- **Contexto y Desafío Identificado**: [Por qué surgió la pregunta a partir del análisis del código o historia]
- **Opciones Evaluadas**:
  - *Opción 1*: [Descripción breve y pros/contras]
  - *Opción 2*: [Descripción breve y pros/contras]
- **Decisión Acordada**: [Opción elegida por el usuario/stakeholder]
- **Justificación Técnica & Regla Resultante**: [Regla de negocio formal resultante]

---

### Bloque B: Roles, Permisos y Seguridad (RBAC & RLS)

#### Pregunta B.1: [Enunciado sobre roles permitidos, permisos y alcance multi-tenant]
- **Contexto**: [Alineación con el catálogo de roles y permisos del proyecto definido en NotebookLM o Base de Datos (ej. admin, manager, user, auditor)]
- **Decisión Acordada**:
  - Roles autorizados para consultar: [Lista de roles autorizados]
  - Roles autorizados para crear/modificar: [Lista de roles con permisos de mutación]
- **Aislamiento Multi-Tenancy**:
  - Columna de aislamiento: `tenant_id` o `company_id` extraída estrictamente del JWT (nunca de query params o body).
  - Regla de RLS aplicable: [Detallar comportamiento de la policy de base de datos]

---

### Bloque C: Experiencia de Usuario (UX) e Interacción Visual

#### Pregunta C.1: [Enunciado sobre interacción en pantalla, navegación o comportamiento UI]
- **Flujo de Pantalla Acordado**: [Paso a paso de lo que ve e interactúa el usuario]
- **Componentes UI Existentes a Reutilizar**: [Componentes en `shared/ui/` identificados]
- **Nuevos Componentes Requeridos**: [Átomos, moléculas u organismos que deben crearse primero bajo el sistema de diseño]
- **Estados Visuales Obligatorios**:
  - **Carga (Loading)**: [Skeleton loader, spinner o placeholder semántico]
  - **Vacío (Empty State)**: [Mensaje y llamada a la acción cuando no hay registros]
  - **Error controlado**: [Feedback visual en caso de fallo HTTP o timeout]
  - **Restricción de permisos**: [Comportamiento visual ante 403 Forbidden]

---

### Bloque D: Casos de Borde, Excepciones y Resiliencia

#### Pregunta D.1: [Enunciado sobre casos límite, concurrencia o validaciones críticas]
- **Escenario Límite / Falla**: [Ej: Datos duplicados, concurrencia simultánea, desconexión de red]
- **Comportamiento Esperado**: [Cómo debe responder el sistema de manera resiliente]
- **Manejo de Errores y Mensajería**: [Mensaje al usuario bajo RFC 7807 y código de error en API]

---

### Bloque E: Persistencia, Integración y Eventos

#### Pregunta E.1: [Enunciado sobre almacenamiento, endpoints, eventos o volumen de datos]
- **Estrategia de Persistencia**: [Nueva tabla vs extensión de tabla existente en PostgreSQL]
- **Lectura / Consulta**: [Dapper / IFinder con .AsNoTracking() o función SQL optimizada]
- **Escritura / Dominio**: [Repositorio EF Core y entidades de dominio en `Core.Domain` con inyección de `IRepository` e `IUnitOfWork`]
- **Eventos de Dominio**: [Eventos emitidos vía MediatR / IPublisher al persistir cambios]

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (PostgreSQL)** | [Tablas, funciones SQL, RLS, índices] | Aislamiento RLS por `tenant_id`/`company_id`, índices optimizados |
| **Backend (.NET 10)** | [CQRS MediatR, FluentValidation, ErrorOr, [HasPermission]] | Onion Architecture pura, inyección de repositorios (sin DbContext en Application) |
| **Frontend (Angular 22+)** | [Componentes Atómicos, Signals, rxResource, *ngxPermissionsOnly] | Atomic Design, Zoneless, OnPush, Tailwind CSS corporativo |
| **App Mobile (si aplica)** | [Pantallas móviles, navegación, offline/online] | Arquitectura móvil desacoplada (o N/A si no aplica) |

---

## 4. Criterios de Aceptación Refinados (Gherkin Enriquecido)

> Estos escenarios sustituyen y amplían los borradores iniciales de la historia de usuario con las definiciones acordadas en la entrevista.

### Escenario 1 — Camino Feliz (Refinado)
- **Dado** [Contexto inicial con datos reales y tenant establecido]
- **Cuando** [Acción ejecutada por el usuario autorizado]
- **Entonces** [Respuesta exitosa, actualización de estado y feedback visual correspondiente]

### Escenario 2 — Caso Alternativo / Flujo Secundario (Refinado)
- **Dado** [Contexto inicial específico]
- **Cuando** [Acción o condición alternativa]
- **Entonces** [Comportamiento controlado y resultado acorde a la regla]

### Escenario 3 — Manejo de Excepción o Caso Borde (Refinado)
- **Dado** [Condición límite o fallo operacional]
- **Cuando** [Ocurre el evento o intento no válido]
- **Entonces** [Respuesta resiliente, código de error ProblemDetails RFC 7807 y mensaje explicativo]

### Escenario 4 — Seguridad y Restricción de Permisos (Refinado)
- **Dado** [Usuario sin el permiso requerido o intento de acceder a datos de otro tenant]
- **Cuando** [Solicita la operación o recurso]
- **Entonces** [Rechazo con HTTP 403 Forbidden o filtrado automático vía RLS sin fuga de información]

---

## 5. Checklist de Preparación para Especificación Técnica

- [ ] Todas las preguntas de negocio fueron respondidas (sin marcadores `[...]` ni dudas pendientes).
- [ ] Los roles y permisos especificados pertenecen al catálogo de la aplicación.
- [ ] El aislamiento multi-tenant se basa estrictamente en claims resueltos desde el JWT (`tenant_id` / `company_id`).
- [ ] Los componentes de UI fueron contrastados con el sistema de diseño en `src/app/shared/ui/`.
- [ ] La arquitectura respeta Onion puro (inyección de `IRepository`, `IDomainService` e `IUnitOfWork`, sin DbContext en Application).
- [ ] El artefacto está guardado como `specs/[KEY]_[slug]/[KEY]-interview.md`.

> **Siguiente Paso**: Proceder con la habilidad `req-technical-spec-generator` para redactar `[KEY].spec.md`, `[KEY]-plan.md` y `[KEY]-tasks.md` tomando esta entrevista y la historia de usuario en Jira como base autoritativa.
