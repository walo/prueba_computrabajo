---
name: req-technical-spec-generator
description: Principal Software Architect skill that analyzes User Stories and Technical Interviews to produce the complete technical breakdown artifact suite (.spec.md, plan.md, tasks.md) strictly aligned with Onion Architecture, CQRS, Atomic Design, and PBAC before any code is written.
---

# Skill: Generador de Especificaciones Técnicas (SDD Breakdown)

Esta habilidad actúa como el **Principal Software Architect**. A partir de la Historia de Usuario gestionada en **Jira Cloud** y de la Entrevista Técnica de refinamiento, produce el trío de artefactos de ingeniería formal (**`.spec.md`**, **`plan.md`**, **`tasks.md`**) dentro de `specs/<KEY>_<slug>/`, garantizando que ningún desarrollador o agente escriba una sola línea de código sin contratos fuertemente tipados y planes validados.

---

## 1. La Suite de Artefactos de Ingeniería Técnica

La Historia de Usuario vive y se rastrea exclusivamente en **Jira Cloud** (para tableros, estados y seguimiento del Product Owner). En el repositorio Git, la carpeta `specs/<KEY>_<slug>/` contiene exclusivamente la suite de ingeniería de software:

```text
specs/<KEY>_<slug>/
├── <KEY>-interview.md     # 1. Entrevista Técnica & Aclaraciones (req-user-story-interviewer)
├── <KEY>.spec.md          # 2. Especificación Técnica, Contratos DTOs & Permisos PBAC (Este Skill)
├── <KEY>-plan.md          # 3. Plan de Arquitectura e Implementación TDD por Fases (Este Skill)
└── <KEY>-tasks.md         # 4. Desglose de Tareas Unitarias Atómicas TDD (Este Skill)
```

---

## 2. Propósito de Cada Artefacto Generado

### A. Especificación Técnica (`US-XXX_<slug>.spec.md`)
* Define contratos DTO bidireccionales (C# records inmutables vs TypeScript interfaces).
* Especifica endpoints RESTful con códigos de respuesta RFC 7807 y permisos exactos `[HasPermission]`.
* Define la arquitectura de componentes UI consumiendo `@accionadev/ui-kit` (`../ui-design-system/ui-manifest.json`) y los Smart Components de la feature.
* Establece la tabla de traducción de errores semánticos para la UI.

### B. Plan de Arquitectura (`US-XXX-plan.md`)
* Desglosa la secuencia de implementación en 6 fases ordenadas: Base de Datos ➔ Backend TDD Rojo ➔ Backend TDD Verde ➔ Frontend UI ➔ Frontend Smart ➔ Verificación Integral.
* Previene el acoplamiento y define las responsabilidades de cada capa.

### C. Desglose de Tareas (`US-XXX-tasks.md`)
* Lista numerada de tareas técnicas atómicas (`TASK-01`, `TASK-02`, etc.) con checkboxes.
* Sirve como guía de ejecución paso a paso durante el workflow de desarrollo.

---

## 3. Plantillas Disponibles (`resources/`)

* [spec_template.md](.agents/skills/req-technical-spec-generator/resources/spec_template.md): Plantilla estándar para `.spec.md`.
* [plan_template.md](.agents/skills/req-technical-spec-generator/resources/plan_template.md): Plantilla para `plan.md`.
* [tasks_template.md](.agents/skills/req-technical-spec-generator/resources/tasks_template.md): Plantilla para `tasks.md`.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Trilogía Completa** — Generados los 3 archivos (`.spec.md`, `plan.md`, `tasks.md`) en la carpeta de la historia.
- [ ] **Contratos Fuertemente Tipados** — DTOs en C# (`record`) y TypeScript (`interface`) con nombres y tipos coincidentes.
- [ ] **Endpoints PBAC** — Todo endpoint cuenta con su permiso explícito (cero `[Authorize]` genéricos).
- [ ] **Consumo de `@accionadev/ui-kit`** — Cero librerías UI de terceros; uso de componentes `<ad-*>` registrados en `../ui-design-system/ui-manifest.json`.
- [ ] **Desacoplamiento Backend** — Handlers especificados con `IRepository`, `IDomainService` e `IUnitOfWork` (cero DbContext).
- [ ] **Tareas con Criterios TDD** — Tareas explícitas de tests en fallo (Rojo) antes de la implementación (Verde).
