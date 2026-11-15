---
name: deliver-pr
description: Release and delivery workflow that executes global test suites, commits atomic conventional changes, pushes to remote, opens a GitHub Pull Request against develop, and transitions the Jira ticket to In Review/Done.
---

# Workflow: Delivery & Pull Request (`deliver-pr.md`)

This workflow governs the final delivery of completed features or bugfixes. It enforces full test execution across the entire solution, creates an atomic conventional commit, generates a GitHub Pull Request, and synchronizes the sprint status in Jira Cloud.

---

## 1. Architectural Governance & Pre-Conditions

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md):
   - Zero tolerance to regressions: 100% of tests must pass.
   - Clean compilation: 0 errors and 0 warnings.
   - Gitflow compliance: Feature branches merge exclusively into `develop`. Direct commits to `main` are strictly forbidden.
2. **Pre-condition:** All feature tasks in `specs/<KEY>_<slug>/<KEY>-tasks.md` marked as completed (`[x]`).

---

## 2. Execution Steps

### Step 1: Global Test Suite & Build Verification
Invoke skills: **`be-dotnet-test`** and **`fe-angular-test`**
1. Run the entire backend test suite:
   ```powershell
   dotnet test
   ```
   *(Must exit with code 0 and 0 failures).*

2. Run the entire frontend test suite:
   ```powershell
   npm test
   ```
   *(Must exit with code 0 and 0 failures).*

3. Verify production compilation:
   ```powershell
   dotnet build --configuration Release
   npm run build
   ```

### Step 2: Knowledge Graph Final Synchronization
Invoke skill: **`graphify`**
1. Sync all AST nodes and relationships:
   ```bash
   graphify update .
   ```

### Step 3: Atomic Conventional Commit
Invoke skill: **`ops-github-gitflow`**
1. Stage all tracked changes (never commit untracked secrets or `.env`):
   ```bash
   git add .
   ```
2. Commit using Conventional Commits format:
   ```text
   feat(<scope>): <description in lowercase>

   - Jira Issue: <KEY>
   - Resolves: specs/<KEY>_<slug>/
   ```
   *Example:*
   ```bash
   git commit -m "feat(quotas): implement organization storage quota limits and alerts"
   ```

### Step 4: Push to Remote & Pull Request Creation
Invoke skill: **`ops-github-gitflow`**
1. Push branch to remote:
   ```bash
   git push origin <current-branch>
   ```
2. Create Pull Request targeting `develop` using `github_client.js`:
   ```bash
   node .agents/skills/ops-github-gitflow/github_client.js create-pr "<current-branch>" "feat(<scope>): <summary>" "develop" "Automated PR generated for <KEY>."
   ```
3. Capture the returned Pull Request URL.

### Step 5: Jira Status Transition & PR Attachment
Invoke skill: **`req-jira`**
1. Transition the Jira issue to **"En revisión" (In Review)** or **"Listo" (Done)**:
   ```bash
   node .agents/skills/req-jira/jira_client.js transition <KEY> "In Review"
   ```
2. Add a comment to the Jira ticket linking the GitHub Pull Request:
   ```bash
   node .agents/skills/req-jira/jira_client.js update <KEY> "Pull Request creado para revisión: <PR_URL>"
   ```

---

## 3. Quality Gate Checklist

- [ ] Global backend test suite (`dotnet test`) passed 100%.
- [ ] Global frontend test suite (`npm test`) passed 100%.
- [ ] Production builds clean with 0 errors.
- [ ] Conventional commit applied.
- [ ] Pull Request opened against `develop` branch.
- [ ] Jira ticket transitioned and updated with PR link.
- [ ] Knowledge graph updated (`graphify update .`).
