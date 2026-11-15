---
name: audit-compliance
description: Security and architecture compliance audit workflow evaluating OWASP Top 10 vulnerabilities, Row Level Security (RLS) multi-tenancy, PBAC endpoint permissions, and token security.
---

# Workflow: Security & Compliance Audit (`audit-compliance.md`)

This workflow performs an exhaustive audit across the codebase, database schemas, and API controllers to guarantee enterprise-grade security, data isolation, and regulatory compliance.

---

## 1. Architectural Governance & Mandates

1. **Governance:** Read [.agents/CONSTITUTION.md](.agents/CONSTITUTION.md) and [.agents/rules/endpoint-permissions-rbac.md](.agents/rules/endpoint-permissions-rbac.md):
   - **Zero generic `[Authorize]`:** Every controller endpoint must enforce its exact permission via `[HasPermission(Permissions.<Feature>.<Action>)]`.
   - **Tenancy Resolution:** `tenant_id` or `company_id` must be extracted exclusively from verified JWT claims. Never trust parameters from query strings or request bodies.
   - **Multi-Tenant RLS:** Tables with tenant columns must have active Row Level Security policies in PostgreSQL.
   - **Token Security:** Refresh tokens must use Refresh Token Rotation (RTR) with reuse detection, storing only SHA-256 hashes.
   - **Clean Onion:** Application Command Handlers must never inject `DbContext`.

---

## 2. Invocation & Scope Resolution (Surgical vs Global)

```text
# Mode 1: Surgical Feature Audit (Default for features & PRs)
/audit-compliance <KEY>
Example: /audit-compliance POJ-12

# Mode 2: Global Solution Audit (Pre-releases or baseline security audit)
/audit-compliance --all
```

1. **Scope Determination:**
   - **Explicit KEY:** If `<KEY>` is supplied, inspect files changed in that feature slice (`git diff --name-only develop...HEAD` or files declared in `specs/<KEY>_<slug>/<KEY>-plan.md`).
   - **Active Feature Branch:** If invoked without arguments but on `feature/<KEY>-<slug>`, automatically default to Surgical mode for that feature.
   - **Main / Develop Branch:** If on `develop` without arguments, prompt the user with `ask_question`: "Do you want to audit a specific Feature (enter KEY) or run a Full Solution Audit (--all)?".
   - **Global (`--all`):** Exhaustively scans all controllers, entities, migrations, and UI components across the repository.

---

## 3. Execution Steps

### Step 1: Endpoint Authorization Audit (PBAC)
Invoke skill: **`ops-security-audit`**
1. Scan target controllers (in Surgical mode, only controllers modified by the feature; in Global mode, all controllers in `Presentation/Web.API/Controllers/`).
2. Verify that:
   - Every public endpoint has explicit `[AllowAnonymous]`.
   - Every protected endpoint has `[HasPermission(...)]`.
   - Any endpoint with a naked `[Authorize]` without policy is flagged as a **CRITICAL** vulnerability.
   - Unauthorized requests return RFC 7807 `403 Forbidden` (not 401).

### Step 2: Multi-Tenancy & Data Isolation Audit (RLS)
1. **Check Project Profile:** Read `PROJECT_PROFILE.md` at the project root (falling back to `.agents/CONSTITUTION.md` Section 2 only if `PROJECT_PROFILE.md` is absent).
   - If the project is marked **Single-Tenant**, mark this Step 2 as **N/A (Not Applicable - Single-Tenant Architecture)** and proceed directly to Step 3.
   - If marked **Multi-Tenant**:
     1. Inspect database migrations and entity configurations (scoped to the feature or globally).
     2. Confirm that all tenant-owned entities have:
        - Non-nullable tenant identifier.
        - PostgreSQL RLS policy enforcing `tenant_id = current_setting('app.current_tenant_id')`.
        - Service layer resolves tenant context strictly from JWT claims via `ICurrentUserService` or `ITenantContext`.

### Step 3: Cryptography & Session Audit (OWASP A01 & A02)
1. Verify token generation uses `System.Security.Cryptography.RandomNumberGenerator` (zero `System.Random`).
2. Audit `RefreshTokenRepository` or authentication service:
   - Tokens stored as SHA-256 hashes.
   - Reuse of an already-used token (`IsUsed == true`) triggers automatic revocation of all active sessions for that user.
   - Blacklist cache handles revoked Access Tokens on logout.

### Step 4: SQL Injection & Input Validation Audit (OWASP A03)
1. Verify all Dapper queries use parameterized inputs (`@Param`).
2. Scan for string concatenation or interpolated SQL strings (`$"SELECT ... WHERE id = '{id}'"` is strictly forbidden).
3. Confirm every Command and Query has an associated `AbstractValidator<T>` with FluentValidation.

### Step 5: Frontend Security & Secret Leakage Audit
1. Verify UI mutation buttons (Create, Edit, Delete) are conditioned with `*ngxPermissionsOnly`.
2. Scan source files, git commits, and `.env.example` to confirm zero hardcoded secrets or credentials.

### Step 6: Generate Formal Audit Report
1. Compile findings using the canonical SEC format:
   ```markdown
   ### Hallazgo SEC-001: [Título de la Vulnerabilidad]
   * **Severidad**: [Crítica 🔴 | Alta 🟠 | Media 🟡 | Baja 🔵]
   * **Ubicación**: [Archivo.cs:Línea](file:///ruta/al/archivo#L10)
   * **Vulnerabilidad**: [CWE / OWASP Category]
   * **Descripción**: [Riesgo e impacto potencial]
   * **Remediación**: [Código corregido seguro]
   ```

---

## 4. Quality Gate Checklist

- [ ] 100% of API endpoints protected with `[HasPermission]`.
- [ ] RLS policies confirmed on all tenant tables in PostgreSQL.
- [ ] Zero raw SQL string interpolations in C#.
- [ ] Refresh token rotation with reuse detection active.
- [ ] Zero secrets in codebase or commit history.
- [ ] All UI action buttons conditioned with `*ngxPermissionsOnly`.
