---
name: req-user-story-creator
description: Business analyst skill for creating, structuring, and publishing standardized User Stories with Gherkin acceptance criteria directly to Jira Cloud, and scaffolding engineering specification suites in specs/<KEY>_<slug>/.
---

# Skill: Creador de Historias de Usuario (Jira Cloud & Gherkin Driven)

Esta habilidad actúa como el **Product Owner / Business Analyst**. Extrae requerimientos funcionales grounded en **NotebookLM**, estructura la Historia de Usuario con criterios de aceptación formales en **Gherkin**, publica el ticket directamente en **Jira Cloud** (fuente única de negocio) y andamia la carpeta de ingeniería técnica en `specs/<KEY>_<slug>/`.

---

## 1. Principio Fundamental: Jira como Fuente Única de la Historia

Para garantizar visibilidad gerencial y evitar duplicación de verdad:
- **Jira Cloud:** Administra el ciclo de vida de la Historia (`Como / Quiero / Para`), criterios Gherkin, estados del Sprint (*To Do ➔ In Progress ➔ Done*) y estimaciones.
- **Git (`specs/<KEY>_<slug>/`):** Administra **exclusivamente** los artefactos de ingeniería de software:
  - `<KEY>-interview.md` (Refinamiento técnico).
  - `<KEY>.spec.md` (Contratos DTOs, endpoints y permisos PBAC con link a Jira).
  - `<KEY>-plan.md` (Plan de fases de arquitectura).
  - `<KEY>-tasks.md` (Checklist de micro-tareas TDD).

> **Cero archivos `.md` de historia duplicados en Git:** La historia oficial y sus criterios viven en Jira.

---

## 2. Herramienta CLI: `new-story.mjs`

El script nativo Node.js sin dependencias administra la creación en Jira y el andamiaje técnico:

```bash
# 1. Crear Historia directamente en Jira Cloud y andamiar carpeta en specs/
node .agents/skills/req-user-story-creator/scripts/new-story.mjs create-jira "Configuración de Cuotas" "@descripcion.md"

# 2. Andamiar manualmente la carpeta de una historia ya existente en Jira
node .agents/skills/req-user-story-creator/scripts/new-story.mjs new "Configuración de Cuotas" --key PROJ-12

# 3. Listar el estado de las especificaciones técnicas locales
node .agents/skills/req-user-story-creator/scripts/new-story.mjs list

# 4. Auditar la completitud de la suite técnica de una feature
node .agents/skills/req-user-story-creator/scripts/new-story.mjs check PROJ-12
node .agents/skills/req-user-story-creator/scripts/new-story.mjs check --all
```

---

## 3. Estructura Canónica de la Historia en Jira (100% Lenguaje de Negocio)

> **REGLA DE ORO (Separación Negocio vs. Ingeniería):**  
> La Historia de Usuario en Jira es un documento **gerencial, funcional y de producto**.  
> **Queda terminantemente prohibido** incluir detalles técnicos de implementación en la HU: cero menciones a `.NET`, `Angular`, `PostgreSQL`, endpoints (`POST /api/...`), códigos HTTP (`400`, `409`, `RFC 7807`), nombres de clases/interfaces (`CommandHandler`, `IRepository`, `FluentValidation`), directivas (`*ngxPermissionsOnly`) o listas de tareas de código. Todo lo técnico vive exclusivamente en `specs/<KEY>_<slug>/` durante la fase de `/spec-generation`.

Toda historia redactada y enviada a Jira debe incluir rigurosamente en lenguaje de negocio:

1. **Definición (Como / Quiero / Para):**
   - **Como:** [Rol del negocio o tipo de usuario]
   - **Quiero:** [Acción o necesidad funcional en el sistema]
   - **Para:** [Beneficio o valor de negocio medible]
2. **Contexto y Reglas de Negocio:** Impacto funcional, políticas del negocio e invariantes del dominio explicadas sin jerga de programación.
3. **Criterios de Aceptación (Gherkin):**
   - Mínimo 3 escenarios completos redactados desde la perspectiva del usuario y del negocio (`Dado / Cuando / Entonces`):
     - Escenario 1: Camino feliz (operación exitosa).
     - Escenario 2: Casos alternativos / límites de reglas de negocio (ej. datos duplicados).
     - Escenario 3: Rechazo controlado / incumplimiento de políticas o permisos de negocio.
4. **Límites Operativos y Reglas Funcionales:** Condiciones de uso, restricciones del negocio y experiencia esperada del usuario.

---

## 4. Recursos de Referencia (`resources/`)

- [ejemplo_historia_completa.md](resources/ejemplo_historia_completa.md): Historia de referencia estándar ("Gold Standard") que ejemplifica la redacción y los criterios Gherkin requeridos antes de publicar en Jira.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Grounded en NotebookLM** — Cero invención de reglas de negocio o cálculos.
- [ ] **Publicada en Jira Cloud** — Ticket creado con Key asignado (ej: `PROJ-12`).
- [ ] **Mínimo 3 Escenarios Gherkin** — Escenarios formales `Dado`, `Cuando`, `Entonces`.
- [ ] **Carpeta de Ingeniería Creada** — Ubicada en `specs/<KEY>_<slug>/`.
- [ ] **Auditoría Limpia** — Salida sin errores de `node .agents/skills/req-user-story-creator/scripts/new-story.mjs check <KEY>`.
