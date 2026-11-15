---
name: story-authoring
description: Requirements authoring workflow that extracts business rules from NotebookLM, structures formal Gherkin acceptance criteria, publishes the User Story directly to Jira Cloud, and scaffolds the engineering directory in specs/<KEY>_<slug>/.
---

# Workflow: User Story Authoring (`story-authoring.md`)

This workflow governs the creation, refinement, and publication of standardized User Stories. **Jira Cloud is the single source of truth** for all business requirements, sprint management, and acceptance criteria. No duplicated `.md` user story files are saved to Git.

---

## 1. Architectural Governance & Grounding

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md). Ensure the story respects domain invariants, multi-tenancy boundaries, and PBAC authorization rules.
2. **Grounding in NotebookLM (Mandatory):**
   - Query NotebookLM (`notebook_query`) for the business process, validation formulas, and constraints.
   - **Missing Rule Protocol:** If a business rule or edge case is missing in NotebookLM, **STOP IMMEDIATELY** and ask the user using `ask_question`. Never assume business logic.

---

## 2. Execution Steps

### Step 1: User Story Formulation (Gherkin-Driven & 100% Business Language)
Invoke skill: **`req-user-story-creator`**
1. **Strict Separation of Concerns (Business vs. Technical):**
   - A User Story is a **100% business and product artifact** meant for Product Owners, QA, and stakeholders in Jira.
   - **STRICTLY FORBIDDEN in the User Story:** Never include programming languages (`.NET`, `Angular`), frameworks, libraries (`FluentValidation`, `EF Core`), architecture patterns (`Onion`, `CQRS`, `Signals`), class/interface names, HTTP status codes (`201`, `409`, `RFC 7807`), API endpoint routes (`POST /api/v1/...`), database table/column names, or technical task checklists. All technical details belong exclusively in the engineering spec suite (`specs/<KEY>_<slug>/`) generated later by `/spec-generation`.
2. Draft the story according to canonical Scrum/Gherkin standards using pure domain and user language:
   - **Definición:** `Como [Rol de Negocio] Quiero [Acción Funcional] Para [Beneficio / Valor de Negocio]`.
   - **Contexto y Reglas de Negocio:** Functional impact, business rules, and domain invariants in plain language.
   - **Criterios de Aceptación (Gherkin):** Minimum 3 complete scenarios written from the user/business perspective:
     - Scenario 1: Happy path (`Dado / Cuando / Entonces`).
     - Scenario 2: Alternative case / business boundary limit.
     - Scenario 3: Controlled rejection / unauthorized access or invalid business condition.
   - **Límites Operativos de Negocio:** Functional constraints, business validations, and expected user-facing behavior.

### Step 2: Publication to Jira Cloud
Invoke skill: **`req-jira`**
1. Use `new-story.mjs create-jira` (or `jira_client.js create`) to publish the User Story directly to Jira Cloud:
   ```bash
   node .agents/skills/req-user-story-creator/scripts/new-story.mjs create-jira "<Título de la Historia>" "<Descripción con Criterios Gherkin>"
   ```
2. The client creates the issue under the configured project key (e.g., `PROJ-12`) with status **"Por hacer" (To Do)**.
3. Capture the returned Jira Issue Key (`<KEY>`) and URL (`https://empresa.atlassian.net/browse/<KEY>`).

### Step 3: Engineering Directory Scaffolding in Git
1. The script automatically scaffolds the technical engineering directory in:
   ```text
   specs/<KEY>_<slug>/
   ```
2. Confirm the directory is ready for the technical refinement phase.
3. Verify directory with:
   ```bash
   node .agents/skills/req-user-story-creator/scripts/new-story.mjs list
   ```

---

## 3. Quality Gate Checklist

- [ ] Story grounded 100% in NotebookLM (zero hallucinated business rules).
- [ ] Published to Jira Cloud with valid Issue Key (e.g., `PROJ-12`).
- [ ] Minimum 3 Gherkin scenarios defined (`Dado / Cuando / Entonces`).
- [ ] 100% pure business language (zero code, endpoints, frameworks, tables, or HTTP codes in the User Story).
- [ ] Local engineering directory created in `specs/<KEY>_<slug>/`.
- [ ] Zero duplicated `.md` user story files in Git.

> **Next Step:** Run `/spec-generation <KEY>` to conduct the technical interview and generate the engineering spec suite.
