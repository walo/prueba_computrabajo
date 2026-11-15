---
name: spec-generation
description: Technical specification workflow that transitions the Jira ticket to In Progress, conducts a Socratic interview with the user, queries the codebase via Graphify, and generates the complete engineering breakdown suite (.spec.md, plan.md, tasks.md).
---

# Workflow: Specification Generation (`spec-generation.md`)

This workflow bridges business requirements with software engineering. It resolves edge cases through a structured Socratic interview, explores existing codebase dependencies with Graphify, and generates the formal engineering suite in `specs/<KEY>_<slug>/` before any code is written.

---

## 1. Architectural Governance & Grounding

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md):
   - **Active Project Profile (`PROJECT_PROFILE.md`):** Read `PROJECT_PROFILE.md` at the project root (falling back to Section 2 of `.agents/CONSTITUTION.md` only if `PROJECT_PROFILE.md` is absent) and strictly adhere to the sealed profile:
     - If marked **Single-Tenant**, NEVER ask for, propose, or generate `TenantId`, organization tables, or PostgreSQL RLS policies. All entities belong directly to `UserId` or are global.
     - If **Mobile** is unmarked, NEVER generate mobile screens, mobile tasks, or mobile-specific DTOs.
   - Strict Onion Architecture: Command Handlers **NEVER** inject `DbContext`. They inject `IRepository`, `IDomainService`, and `IUnitOfWork`.
   - Read Model: Queries use `IFinder` with `.AsNoTracking()`.
   - Granular PBAC: Every endpoint requires `[HasPermission(Permissions.<Feature>.<Action>)]`.
   - Frontend: Standalone Angular 22+, Zoneless, Signals, `rxResource`, and reusable `<ad-*>` components from `@accionadev/ui-kit` (`../ui-design-system/ui-manifest.json`).

---

## 2. Invocation & Target Story Resolution

The workflow accepts the Jira issue key as a parameter:
```text
/spec-generation <KEY>
Example: /spec-generation POJ-12
```

1. **Jira Key Verification:**
   - If `<KEY>` is supplied in the command or user message, validate format (`[PROJECT]-[NUMBER]`).
   - If `<KEY>` is omitted, prompt the user using `ask_question` to input the ticket key or select from active Jira issues.

---

## 3. Execution Steps

### Step 1: Retrieve Story & Requirements from Jira Cloud
Invoke skill: **`req-jira`**
1. Fetch full issue details from Jira Cloud:
   ```bash
   node .agents/skills/req-jira/jira_client.js get <KEY>
   ```
2. Extract the issue Summary, Description, and Gherkin Acceptance Criteria.
3. Formulate the URL-friendly `<slug>` from the summary (e.g., `POJ-12` + "Organization Quotas" ➔ `POJ-12_organization-quotas`).

### Step 2: Gitflow Feature Branch Creation
Invoke skill: **`ops-github-gitflow`**
1. Ensure the working tree is clean and synchronized on `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   ```
2. Create and checkout the dedicated feature branch:
   ```bash
   git checkout -b feature/<KEY>-<slug>
   ```
   *(All subsequent specification artifacts and code will live in this isolated branch).*

### Step 3: Jira Ticket Transition to "In Progress"
Invoke skill: **`req-jira`**
1. Transition the Jira issue to **"En curso" (In Progress)**:
   ```bash
   node .agents/skills/req-jira/jira_client.js transition <KEY> "In Progress"
   ```

### Step 4: Codebase Context via Graphify (Surgical Input)
Invoke skill: **`graphify`**
1. If the feature modifies or extends existing domain models, find contracts and dependencies using:
   ```bash
   graphify query "<FeatureName>"
   ```
2. Read **only** the specific files or interfaces flagged by Graphify. Never read the whole codebase.

### Step 5: Socratic Technical Interview
Invoke skill: **`req-user-story-interviewer`**
1. Initialize the interview document in the feature specs folder:
   ```bash
   node .agents/skills/req-user-story-interviewer/scripts/interview-helper.mjs init <KEY>
   ```
2. Conduct the interview addressing:
   - **Domain & Edge Cases:** Concurrency, boundaries, data retention.
   - **Roles & PBAC:** Required granular permissions and tenancy resolution (`tenant_id`/`company_id` from JWT).
   - **UI & UX (`@accionadev/ui-kit`):** Inspect `../ui-design-system/ui-manifest.json` to identify which `<ad-*>` components will be reused and whether any new `<ad-*>` component must be delegated to `../ui-design-system`, plus loading, empty, and error states.
   - **Mobile UX:** Touch navigation, camera/offline (if story includes Mobile).
3. Use `ask_question` whenever architectural or functional decisions must be confirmed with the user.
4. Validate interview document quality:
   ```bash
   node .agents/skills/req-user-story-interviewer/scripts/interview-helper.mjs check <KEY>
   ```

### Step 6: Technical Specification Suite Generation
Invoke skill: **`req-technical-spec-generator`**
Generate the 3 formal engineering artifacts in `specs/<KEY>_<slug>/`:

1. **`<KEY>.spec.md` (Technical Specification):**
   - Header linking to Jira Issue: `[KEY](https://tu-empresa.atlassian.net/browse/<KEY>)`.
   - Typed contracts: Bidirectional C# `record` vs TypeScript `interface`.
   - Endpoints table: HTTP Verb, Route, RFC 7807 codes, and `[HasPermission]`.
   - Backend Domain & Application slice design.
   - Frontend Atomic Design components & UI Error Translation catalog.
   - Mobile UI specification (if Mobile component is declared).

2. **`<KEY>-plan.md` (Implementation Architecture Plan):**
   - 6 sequential phases: Persistence ➔ Backend TDD Red ➔ Backend TDD Green ➔ Frontend UI ➔ Frontend Smart ➔ Verification.

3. **`<KEY>-tasks.md` (Atomic TDD Task Checklist):**
   - Ordered micro-tasks (`TASK-01`... `TASK-N`) with explicit Red before Green testing steps.
   - Optional Mobile tasks (`TASK-M01`... `TASK-MN`) if the story includes Mobile.

### Step 7: Engineering Suite Audit & Spec Commit
Invoke skill: **`req-user-story-creator`**
1. Audit the specification folder to ensure full compliance:
   ```bash
   node .agents/skills/req-user-story-creator/scripts/new-story.mjs check <KEY>
   ```
2. Confirm all 4 files are present and green:
   ```bash
   node .agents/skills/req-user-story-creator/scripts/new-story.mjs list
   ```
3. Leave the specification suite ready on the feature branch `feature/<KEY>-<slug>` without committing automatically (per `.agents/rules/git-commit-policy.md`, commits are only created when explicitly requested by the user or during `/deliver-pr`).
   *(Branch `feature/<KEY>-<slug>` remains checked out and ready for `/feature-scaffold`).*

---

## 4. Quality Gate Checklist

- [ ] Git feature branch `feature/<KEY>-<slug>` created and branched from latest `develop`.
- [ ] Jira ticket moved to "In Progress".
- [ ] `<KEY>-interview.md` completed with 0 placeholders (`[...]`) and passes `interview-helper.mjs check`.
- [ ] `<KEY>.spec.md` defines typed C#/TS contracts and `[HasPermission]` on all endpoints.
- [ ] UI Error Translation table defined with semantic user messages.
- [ ] `<KEY>-plan.md` and `<KEY>-tasks.md` prepared with strict TDD phases.
- [ ] Suite passes `new-story.mjs check <KEY>` with exit code 0.
- [ ] Spec suite committed into feature branch.

> **Next Step:** Run `/feature-scaffold <KEY>` to execute the TDD implementation cycle (Red ➔ Green).
