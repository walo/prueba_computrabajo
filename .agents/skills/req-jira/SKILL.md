---
name: req-jira
description: Cloud requirements and project tracking skill for creating, updating, and querying business User Stories and issues in Atlassian Jira Cloud. Use whenever generating stories, updating sprint progress, or synchronizing tickets.
---

# Skill: Jira Integration (Atlassian Jira Cloud API)

Automatiza la gestión, creación, consulta y transición de Historias de Usuario e incidencias en **Atlassian Jira Cloud** mediante el cliente nativo sin dependencias externas `jira_client.js`.

---

## 🔐 Configuración de Variables de Entorno

El cliente lee automáticamente las credenciales desde el archivo `.env` en la raíz del repositorio (o variables de entorno del sistema):

```env
JIRA_BASE_URL=https://tu-empresa.atlassian.net
JIRA_USER_EMAIL=usuario@empresa.com
JIRA_API_TOKEN=tu_api_token_de_jira
JIRA_PROJECT_KEY=PROJ
```
> Consulta [.env.example](.env.example) como plantilla oficial.



---

## 🛠️ Comandos de CLI Disponibles

### 1. Crear Historia o Issue
Crea una nueva incidencia en Jira. Soporta texto directo o ruta a un archivo Markdown precedido por `@`:

```bash
# Con texto directo
node .agents/skills/req-jira/jira_client.js create "<Resumen Claro>" "<Descripción con Criterios de Aceptación>" "Story"

# Con archivo Markdown (recomendado para historias detalladas)
node .agents/skills/req-jira/jira_client.js create "Gestión de Catálogo de Items" "@specs/US-001_items/US-001.md" "Story"
```
**Respuesta JSON esperada:**
```json
{ "status": "success", "key": "PROJ-12", "url": "https://tu-empresa.atlassian.net/browse/PROJ-12" }
```

### 2. Consultar Issue
Obtiene el detalle actual (resumen, estado, descripción procesada):

```bash
node .agents/skills/req-jira/jira_client.js get <ISSUE_KEY>
```

### 3. Actualizar Issue
Actualiza la descripción y opcionalmente el resumen:

```bash
node .agents/skills/req-jira/jira_client.js update <ISSUE_KEY> "<Nueva Descripción o @archivo.md>" ["Nuevo Resumen"]
```

### 4. Transicionar Estado (Workflow de Sprint)
Mueve el issue a través de las columnas del tablero (ej. "En curso", "En revisión", "Finalizado"):

```bash
node .agents/skills/req-jira/jira_client.js transition <ISSUE_KEY> "En curso"
```

### 5. Añadir Comentario
Agrega notas de avance o enlaces a Pull Requests:

```bash
node .agents/skills/req-jira/jira_client.js comment <ISSUE_KEY> "Especificación técnica completada en docs/specs/HU-PROJ-12.md"
```

---

## 🏛️ Directivas Mandatorias de Redacción en Jira

1. **Grounded en NotebookLM (Prerrequisito Innegociable):** Solo se crean o actualizan historias si la información proviene de una consulta exitosa al cuaderno oficial de NotebookLM. Si la sesión expira o no responde, detener el flujo de inmediato.
2. **Enfoque 100% Funcional (El "¿Qué?"):** Redacción estricta desde la perspectiva del usuario y del negocio.
3. **Cero Jerga Técnica en el Ticket:** Prohibido mencionar nombres de frameworks (.NET, Angular), librerías (MediatR, Tailwind, ngx-permissions), nombres de tablas SQL o patrones de código en el resumen o descripción en Jira.
4. **Estructura Canónica Obligatoria:**
   - **Historia:** `Como [rol de negocio], quiero [acción/funcionalidad], para [beneficio esperado]`.
   - **Criterios de Aceptación (Gherkin):** Escenarios `Dado / Cuando / Entonces` cubriendo flujos exitosos y restricciones de negocio.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Credenciales Configuradas** — `.env` con `JIRA_BASE_URL`, `JIRA_USER_EMAIL`, `JIRA_API_TOKEN` y `JIRA_PROJECT_KEY`.
- [ ] **Grounding Verificado** — La historia está respaldada por NotebookLM; cero requerimientos inventados.
- [ ] **Formato Gherkin Válido** — Criterios con `Dado`, `Cuando`, `Entonces`.
- [ ] **Cero Contaminación Técnica** — Cero menciones a clases, tablas o frameworks en la descripción de Jira.
- [ ] **Respuesta `status: success`** — Ejecución exitosa de `jira_client.js` con confirmación de clave y URL generada.
