---
name: knowledge-graph
description: Codebase knowledge graph navigation and synchronization workflow that leverages Graphify to query architecture, trace dependencies, and inspect relationships with minimal token usage.
---

# Workflow: Knowledge Graph Navigation (`knowledge-graph.md`)

This workflow governs codebase exploration, architecture discovery, and persistent dependency graph synchronization using **Graphify**. It prevents blind file reading and massive context saturation by providing scoped subgraphs and precise call paths.

---

## 1. Architectural Governance & Mandates

1. **Governance:** Read [.agents/rules/graphify.md](.agents/rules/graphify.md) and [.agents/rules/token-optimization.md](.agents/rules/token-optimization.md):
   - **Graphify First:** Before reading multiple raw code files to "understand" an architecture, query the graph first.
   - **Surgical Queries:** Prohibit ultra-generic queries (e.g., `"class"` or `"service"`). Use exact class, method, or interface identifiers (e.g., `"IUnitOfWork"`, `"CreateItemCommandHandler"`).
   - **Zero API Cost:** AST-based updates execute locally via CLI without external LLM API costs.

---

## 2. Execution Steps

### Step 1: Verify Knowledge Graph Status
1. Check if `graphify-out/graph.json` exists in the repository root.
2. If the graph does not exist or files were recently modified, re-index the repository:
   ```bash
   graphify update .
   ```

### Step 2: Architecture Querying & Subgraph Inspection
Invoke skill: **`graphify`**
1. **Targeted Concept Query:**
   To understand a specific component, handler, or entity and its direct callers:
   ```bash
   graphify query "<ComponentName>"
   ```
2. **Relationship & Path Tracing:**
   To discover how two distant architectural components communicate (e.g., Controller to Repository):
   ```bash
   graphify path "<OriginClass>" "<DestinationClass>"
   ```
3. **Concept Explanation:**
   To retrieve a focused summary of an isolated cluster:
   ```bash
   graphify explain "<ModuleOrConcept>"
   ```

### Step 3: Architecture Report Inspection (Broad Scope)
1. If broad architectural community detection is required, review:
   `graphify-out/GRAPH_REPORT.md`
2. Navigate via `graphify-out/wiki/index.md` if available instead of reading raw files.

### Step 4: Post-Refactor Re-Indexing
1. Whenever code files are added, moved, or deleted during a task, finalize by refreshing the graph:
   ```bash
   graphify update .
   ```

---

## 3. Quality Gate Checklist

- [ ] Codebase explored via `graphify query` or `graphify path` before reading raw files.
- [ ] Subgraphs scoped to the minimal necessary context.
- [ ] Knowledge graph refreshed with `graphify update .` after changes.
