---
name: ops-security-audit
description: Security Architect and Compliance Auditor skill for evaluating vulnerabilities, OWASP Top 10 compliance, Row Level Security (RLS) enforcement, PBAC authorization checks, token rotation (RTR), and secret leakage across backend and frontend codebases.
---

# Skill: Auditoría de Seguridad, Cumplimiento y Protección de Datos

Esta habilidad actúa como el **Security Architect & Compliance Auditor**. Audita exhaustivamente el código fuente, la infraestructura de datos y las APIs para garantizar cumplimiento riguroso de OWASP Top 10, políticas de aislamiento multi-tenant (RLS), autorización granular (PBAC) y prevención de fuga de credenciales.

---

## 1. Dominios de Auditoría Mandatorios

### A. Aislamiento Multi-Tenant y Prevención de Spoofing
* **Resolución Estricta por JWT:** El `tenant_id` o `company_id` debe resolverse **exclusivamente** desde los claims verificados del token criptográfico (`ICurrentUserService`). Queda terminantemente prohibido confiar en identificadores recibidos por URL (query params) o en el cuerpo de peticiones de mutación.
* **Row Level Security (RLS):** Toda tabla con partición por tenant debe contar con políticas RLS activas en PostgreSQL (`SECURITY INVOKER`), asegurando que ninguna consulta Dapper o EF Core eluda el aislamiento.

### B. Gestión de Sesiones y Protección Criptográfica (OWASP A01 & A02)
* **Refresh Tokens con RTR:** Verificar que la base de datos guarde únicamente el hash SHA-256 del refresh token y que ante la detección de reutilización (`IsUsed == true`) se revoquen atómicamente todas las sesiones del usuario.
* **Blacklist de Tokens en Logout:** Validar que el logout registre el `jti` en caché (Redis/Memory) con TTL igual a la vida remanente del Access Token.
* **Generación Criptográfica:** Uso estricto de `System.Security.Cryptography.RandomNumberGenerator`. Prohibido el uso de `System.Random` para tokens, OTPs o claves.
* **Cero Secretos Hardcodeados:** Claves JWT, connection strings o API tokens deben inyectarse exclusivamente vía variables de entorno (`.env` / Secret Managers).

### C. Autorización Granular en Endpoints (PBAC)
* **Cero `[Authorize]` Genéricos:** Todo endpoint de negocio debe exigir su permiso específico mediante `[HasPermission(Permissions.<Feature>.<Action>)]`.
* **Rechazo RFC 7807:** Si el usuario carece del permiso, la respuesta debe ser `403 Forbidden` estructurada con `ProblemDetails` y código semántico de error.

### D. Inyección y Sanitización de Datos (OWASP A03)
* **Cero SQL Concatenado:** Todas las consultas dinámicas con Dapper deben usar exclusivamente parámetros tipados (`@Param`).
* **Validación Declarativa:** Todo comando o query externa debe estar respaldada por un validador de `FluentValidation` antes de alcanzar el Handler.

### E. Frontend Shielding (Angular)
* **Ocultamiento Declarativo:** Acciones de mutación (botones de creación, edición, eliminación) condicionadas con `*ngxPermissionsOnly`.
* **Guards de Navegación:** Rutas protegidas con `authGuard` y `permissionGuard`.
* **Storage Seguro:** Cero almacenamiento de secretos sensibles en `localStorage` no protegido contra XSS.

---

## 2. Formato Canónico de Reporte de Hallazgos

Cuando se presente una auditoría, estructurar cada vulnerabilidad identificada bajo este formato:

```markdown
### Hallazgo SEC-001: [Título Conciso de la Vulnerabilidad]
* **Severidad**: [Crítica 🔴 | Alta 🟠 | Media 🟡 | Baja 🔵]
* **Ubicación**: [Archivo.cs:Líneas](file:///ruta/al/archivo#L10-L20)
* **Vulnerabilidad (CWE/OWASP)**: [ej. OWASP A03:2021 - SQL Injection / CWE-89]
* **Descripción del Riesgo**: [Explicación del impacto y vector de ataque potencial]
* **Remediación Inmediata**:
  ```csharp
  // Código corregido seguro
  ```
```

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Aislamiento de Tenant Verificado** — `company_id`/`tenant_id` resuelto desde claims; cero spoofing.
- [ ] **RLS Activo** — Políticas aplicadas en base de datos PostgreSQL.
- [ ] **PBAC en el 100% de Endpoints** — Todos los controladores decorados con `[RequirePermission]`.
- [ ] **RTR y Hashes SHA-256** — Refresh tokens protegidos contra robo y secuestro de sesión.
- [ ] **Cero Parámetros SQL Concatenados** — Consultas Dapper 100% parametrizadas.
- [ ] **Cero Secretos en Repositorio** — Tokens y claves referenciados desde `.env` e incluidos en `.gitignore`.
