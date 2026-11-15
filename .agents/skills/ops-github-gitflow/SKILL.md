---
name: ops-github-gitflow
description: Version control and Gitflow specialist skill for branching (feature, release, hotfix), atomic conventional commits, merge strategies (--no-ff), semantic tagging, and Pull Request automation via GitHub CLI (gh) or native API client. Use whenever branching, committing, creating PRs, or releasing.
---

# Skill: Git, Gitflow Estricto y Automatización de GitHub

Gobierna de manera rigurosa todas las operaciones de control de versiones, ciclo de ramas y entrega de software en el ecosistema empresarial.

---

## 1. Estructura y Rol de las Ramas

### A. Ramas Principales (Infinitas)
* **`main`**: Representa el estado oficial de producción. Cero commits directos. Todo merge a `main` proviene de `release/*` o `hotfix/*` y debe acompañarse de un tag semántico (`vX.Y.Z`).
* **`develop`**: Rama de integración continua. El código consolidado y probado vive aquí. Todo merge proviene de `feature/*` o `hotfix/*`.

### B. Ramas Temporales
* **`feature/<ID-o-nombre>`**: Ramas de desarrollo de características. Nacen de `develop` actualizado y se integran exclusivamente de vuelta a `develop`.
* **`release/v<version>`**: Ramas de preparación para despliegues a producción. Nacen de `develop` y se fusionan en `main` y en `develop`.
* **`hotfix/<ID-o-nombre>`**: Corrección urgente de bugs críticos en producción. Nacen de `main` y se fusionan en `main` y `develop`.

---

## 2. Convención de Commits Atómicos (Conventional Commits)

Cada commit debe ser semántico, específico y atómico:

```text
<tipo>(<alcance-opcional>): <descripción concisa en minúsculas>
```

| Tipo | Propósito | Ejemplo |
| :--- | :--- | :--- |
| **`feat`** | Nueva funcionalidad | `feat(auth): implement refresh token rotation with reuse detection` |
| **`fix`** | Corrección de un defecto o bug | `fix(cart): prevent negative quantity on item removal` |
| **`refactor`**| Refactorización sin alterar comportamiento | `refactor(domain): decouple handler from db context using unit of work` |
| **`test`** | Adición o corrección de pruebas | `test(items): add unit tests for create item command handler` |
| **`docs`** | Documentación o especificaciones | `docs(specs): add technical specification for feature 002` |
| **`chore`** | Mantenimiento, dependencias, build | `chore(deps): update fluentvalidation to 11.10.0` |

---

## 3. Automatización de Pull Requests (PR)

### Opción A: Mediante GitHub CLI (`gh`)
Si la herramienta oficial `gh` está instalada y autenticada en el entorno:

```bash
# Crear PR contra develop
gh pr create --base develop --title "feat(core): implement item management" --body-file docs/specs/PR_BODY.md

# Ver estado de PRs
gh pr status

# Revisar y fusionar PR sin fast-forward
gh pr merge <PR_NUMBER> --merge --delete-branch
```

### Opción B: Mediante Script Nativo (`github_client.js`)
Si se ejecuta en entornos donde `gh` no está disponible, el cliente nativo Node.js sin dependencias utiliza `GITHUB_TOKEN` desde `.env`:

```bash
# Crear Pull Request
node .agents/skills/ops-github-gitflow/github_client.js pr-create <head-branch> "<título>" "develop" "<descripción-o-@archivo.md>"

# Obtener PR
node .agents/skills/ops-github-gitflow/github_client.js pr-get <PR_NUMBER>

# Comentar en PR
node .agents/skills/ops-github-gitflow/github_client.js pr-comment <PR_NUMBER> "Revisión aprobada por CI/CD"
```

---

## 4. Estrategia de Fusiones (Merge con `--no-ff`)

Para mantener el historial trazable en el grafo de git, todas las fusiones de ramas terminadas deben usar `--no-ff`:

```bash
git checkout develop
git pull origin develop
git merge --no-ff feature/nueva-funcionalidad -m "merge: incorporate feature/nueva-funcionalidad into develop"
git push origin develop
```

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Rama Correcta** — Se trabaja sobre una rama `feature/*`, `release/*` o `hotfix/*`; cero trabajo directo en `main` o `develop`.
- [ ] **Tests Locales en Verde** — La suite de pruebas de la feature pasa al 100% antes de hacer commit.
- [ ] **Commit Semántico** — Mensaje bajo el estándar Conventional Commits (`feat:`, `fix:`, `refactor:`, etc.).
- [ ] **Cero Archivos Sensibles** — `.env`, credenciales, `bin/`, `obj/` y `node_modules/` verificados en `.gitignore`.
- [ ] **Rama Remota Actualizada** — `git pull origin develop` ejecutado para prevenir conflictos antes de solicitar PR.
- [ ] **PR Documentado** — PR generado con título claro, descripción de cambios y ticket/issue asociado.
