---
name: bugfix-forensic
description: Forensic bug diagnosis and surgical resolution workflow that reproduces defects with a failing test in Red, applies minimal fixes, and verifies Green with zero regressions.
---

# Workflow: Forensic Bugfix (`bugfix-forensic.md`)

This workflow governs the surgical identification and resolution of defects. **No bugfix is applied without first proving the defect via an automated failing test (Red)** to guarantee permanent regression protection.

---

## 1. Architectural Governance & Pre-Conditions

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md):
   - Prohibit monkey patching or masking errors with empty `catch` blocks.
   - Handlers must continue to adhere to Onion Architecture (no `DbContext` in Application).
   - Domain rules must be guarded inside Entities and Domain Services.
   - Never remove or weaken `[HasPermission]` authorization checks to "make something work".

---

## 2. Execution Steps

### Step 1: Forensic Root Cause Analysis
Invoke skill: **`graphify`**
1. Locate callers, dependencies, and execution paths using Graphify:
   ```bash
   graphify path "<CallerComponent>" "<FailingComponent>"
   ```
2. Read the exact lines and error stack traces to understand the root cause.

### Step 2: Write Reproduction Test (Red Phase)
Invoke skills: **`be-dotnet-test`** or **`fe-angular-test`**
1. Author a dedicated unit or integration test reproducing the exact failure:
   - For backend: Create a test method `Should_Fail_When_[Scenario]` in `tests/`.
   - For frontend: Create an `it('should handle [error scenario]')` test in `*.spec.ts`.
2. Run the focalized test to confirm failure (**Red**):
   ```bash
   dotnet test --filter FullyQualifiedName~[BugName]
   # or
   cmd.exe /c npm test -- -t "[BugDescription]"
   ```
   *(The test MUST fail. If it passes, the test does not accurately reproduce the bug).*

### Step 3: Surgical Fix Implementation (Green Phase)
Invoke skills: **`be-dotnet-feature`** or **`fe-angular-feature`**
1. Apply the minimal, surgical code modification needed to satisfy the test.
2. Re-run the focalized test until it passes (**Green**):
   ```bash
   dotnet test --filter FullyQualifiedName~[BugName]
   ```

### Step 4: Security & Regression Verification
Invoke skills: **`ops-security-audit`** and **`graphify`**
1. Verify no security vulnerabilities or authorization loopholes were introduced.
2. Run the full test suite for the affected module to ensure zero regressions.
3. Update the knowledge graph:
   ```bash
   graphify update .
   ```

---

## 3. Quality Gate Checklist

- [ ] Defect reproduced with an automated test in Red before editing production code.
- [ ] Fix applied surgically with minimal line diffs.
- [ ] Reproduction test passes in Green.
- [ ] Zero regressions in related unit tests.
- [ ] Knowledge graph updated (`graphify update .`).

> **Next Step:** Run `/deliver-pr` to compile, verify, and deliver the fix via Gitflow.
