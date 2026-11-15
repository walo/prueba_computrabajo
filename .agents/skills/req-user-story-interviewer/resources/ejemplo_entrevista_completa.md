# Entrevista Técnica y de Negocio: Configuración de Límites de Almacenamiento por Organización

**Historia ID / Jira Issue**: PROJ-12  
**Jira URL**: https://tu-empresa.atlassian.net/browse/PROJ-12  
**Fecha de la Sesión**: 2026-10-23  
**Entrevistador**: Antigravity Assistant (AI Systems Architect)  
**Entrevistado (Stakeholder/PO/Dev)**: Product Owner / Enterprise Architecture Lead  
**Estado Jira**: En Curso (In Progress)  
**Historia de Usuario (Criterios de Aceptación)**: Centralizada y gestionada en Jira Cloud  

---

## 1. Resumen Ejecutivo y Objetivos Clarificados

### 1.1 Propósito y Valor en la Jerarquía
Permitir a los administradores de organización (`admin`, `manager`) configurar cuotas máximas de almacenamiento por sede o departamento y recibir notificaciones tempranas cuando el consumo alcance niveles críticos (90%). Resuelve el problema de saturación imprevista de infraestructura y permite planificar la ampliación de recursos.
Opera estrictamente en el contexto de la jerarquía `Tenant → Organización / Sede → Recursos`, asegurando que ninguna cuota o métrica sea visible ni modificable por usuarios de otro tenant (`tenant_id`).

### 1.2 Alcance Acordado
- **Dentro del alcance (In-Scope)**:
  - Configuración del límite en Gigabytes (GB) por organización.
  - Generación de evento de alerta e inserción en tabla de notificaciones cuando el consumo supere el 90%.
  - Notificación en tiempo real al panel de administración en Angular.
  - Bloqueo preventivo de nuevas subidas de archivos cuando el consumo alcance el 100%.
- **Fuera del alcance explícito (Out-of-Scope)**:
  - Pasarela de pago o cobro automático por almacenamiento adicional (se reservará para una historia de facturación posterior).
  - Eliminación automática de archivos antiguos o políticas de retención.

---

## 2. Registro de Preguntas y Resoluciones Técnicas de Negocio

### Bloque A: Dominio y Reglas de Negocio

#### Pregunta A.1: ¿Cómo se determina formalmente que una organización ha superado el umbral de alerta?
- **Contexto y Desafío Identificado**: El almacenamiento se acumula archivo por archivo. Calcular la suma total de bytes en disco en cada petición puede ser costoso si hay millones de registros.
- **Opciones Evaluadas**:
  - *Opción 1*: Ejecutar un `COUNT/SUM` directo sobre la tabla de archivos en cada subida.
  - *Opción 2*: Mantener un campo agregado `consumed_bytes` en la tabla de cuotas actualizado mediante eventos de dominio e invocar una función SQL optimizada para verificar si `consumed_bytes >= (quota_limit_bytes * 0.90)`.
- **Decisión Acordada**: Opción 2.
- **Justificación Técnica & Regla Resultante**: El agregador `consumed_bytes` se actualiza de forma atómica. Al superar el 90%, el Domain Service emite el evento de dominio `OrganizationQuotaThresholdExceededEvent`.

---

#### Pregunta A.2: ¿Qué sucede si dos usuarios suben archivos en paralelo y ambos exceden el límite al mismo tiempo?
- **Contexto**: Concurrencia en la validación de cuota máxima.
- **Decisión Acordada**: Se aplica control de concurrencia optimista a nivel de base de datos. Si una transacción detecta que la cuota llega al 100%, la segunda operación es rechazada con un código de dominio `QuotaExceededError`.
- **Regla Resultante**: El comando `UploadFileCommand` valida previamente la cuota disponible antes de autorizar el almacenamiento físico.

---

### Bloque B: Roles, Permisos y Seguridad (RBAC & RLS)

#### Pregunta B.1: ¿Qué roles y permisos tienen autorización para modificar cuotas y visualizar alertas?
- **Contexto**: Alineación con el modelo de autorización granular por permisos (PBAC) y catálogo de base de datos.
- **Decisión Acordada**:
  - **Permiso de lectura**: `Quotas.Read` (asignado a roles `admin`, `manager`, `auditor`).
  - **Permiso de modificación**: `Quotas.Update` (exclusivo para rol `admin`).
  - **Permiso de recepción de alertas**: `Notifications.Read`.
- **Aislamiento Multi-Tenancy**:
  - Columna de aislamiento: `tenant_id` resuelta estrictamente desde los claims del JWT (nunca de la URL o payload).
  - Política de RLS:
    ```sql
    CREATE POLICY organization_quotas_tenant_isolation ON organization_quotas
      FOR ALL USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
    ```

---

### Bloque C: Experiencia de Usuario (UX) e Interacción Visual

#### Pregunta C.1: ¿Cómo debe presentarse el estado de la cuota en el panel de Angular?
- **Flujo de Pantalla Acordado**:
  1. La pantalla de administración muestra una tarjeta de métrica con barra de progreso circular o lineal.
  2. Cuando el consumo es `< 75%`: barra de color primario (`var(--color-primary)`).
  3. Entre `75%` y `90%`: barra de color advertencia (`var(--color-warning)`).
  4. Mayor a `90%`: barra de color crítico (`var(--color-danger)`) con aviso destacado.
  5. El botón "Modificar Cuota" solo es visible si el usuario cuenta con el permiso `*ngxPermissionsOnly="['Quotas.Update']"`.
- **Componentes UI Existentes a Reutilizar**:
  - Átomos: `ButtonComponent` (`shared/ui/atoms/button/`), `BadgeComponent`.
  - Moléculas: `StatCardComponent`, `ProgressBarComponent`.
  - Organismos: `SettingsCardComponent`.
- **Nuevos Componentes Requeridos**:
  - Ninguno; se componen los átomos y moléculas existentes bajo el sistema de diseño.
- **Estados Visuales Obligatorios**:
  - **Carga**: Skeleton placeholder en las métricas de almacenamiento.
  - **Vacío**: "Sin límites asignados (cuota ilimitada por defecto)".
  - **Error**: Notificación toast con RFC 7807 ProblemDetails.

---

### Bloque D: Casos de Borde, Excepciones y Resiliencia

#### Pregunta D.1: ¿Qué ocurre si un usuario elimina un archivo de gran tamaño?
- **Escenario Límite**: La cuota estaba en 95% (en estado de alerta activa) y la eliminación reduce el uso al 70%.
- **Decisión Acordada**: El evento `FileDeletedDomainEvent` decrementa el contador `consumed_bytes`. Si el porcentaje cae por debajo del 90%, el estado de alerta activa se desactiva automáticamente.
- **Justificación Técnica**: Mantiene sincronizado el estado visual en el dashboard sin intervención manual de soporte.

---

### Bloque E: Persistencia, Integración y Eventos

#### Pregunta E.1: ¿Dónde se almacenan las cuotas y cómo se consultan?
- **Estrategia de Persistencia**:
  - Tabla `organization_quotas` vinculada a `organization_id` y `tenant_id`.
- **Lectura / Consulta**:
  - Consulta optimizada mediante Dapper invocando el procedimiento o vista indexada de cuotas.
- **Escritura / Dominio**:
  - Entity Framework Core en `Infrastructure/Persistence/Repositories/OrganizationQuotaRepository.cs`.
  - Entidad de dominio en `Core.Domain/Entities/OrganizationQuota.cs`.
- **Eventos**:
  - MediatR emite `OrganizationQuotaThresholdExceededDomainEvent` que el manejador despacha hacia el hub de notificaciones.

---

## 3. Matriz de Decisiones Técnicas por Stack

| Stack / Capa | Decisión Técnica Acordada | Justificación / Patrón Aplicado |
|---|---|---|
| **Base de Datos (PostgreSQL)** | Tabla `organization_quotas` con RLS por `tenant_id` y función SQL para agregados. | Aislamiento multi-tenant mandatorio y cero sobrecarga de cálculo en memoria. |
| **Backend (.NET 10)** | Command `UpdateOrganizationQuotaCommand`, Query `GetOrganizationQuotaQuery`, FluentValidation, ErrorOr. | Clean/Onion Architecture, CQRS con MediatR, inyección limpia de `IRepository` y `IUnitOfWork`. |
| **Frontend (Angular 22+)** | Signals reactivas en `QuotaStore`, protección de ruta con `permissionGuard`, directiva `*ngxPermissionsOnly`. | Zoneless, diseño atómico, tokens semánticos de Tailwind CSS. |

---

## 4. Criterios de Aceptación Refinados (Gherkin Enriquecido)

### Escenario 1 — Camino Feliz: Configuración exitosa de cuota
- **Dado** que un usuario autenticado cuenta con el permiso `Quotas.Update`
- **Y** se encuentra en el formulario de configuración de la organización
- **Cuando** ingresa un nuevo límite válido mayor al consumo actual y presiona "Guardar"
- **Entonces** el sistema actualiza el registro en `organization_quotas`
- **Y** la interfaz emite un toast de confirmación exitosa.

### Escenario 2 — Caso Alternativo: Rechazo por cuota inferior al consumo
- **Dado** que la organización consume actualmente `350 GB`
- **Cuando** el administrador intenta fijar el límite en `300 GB`
- **Entonces** FluentValidation rechaza la solicitud antes de tocar la base de datos
- **Y** la API retorna HTTP 400 ProblemDetails con el mensaje "El límite no puede ser inferior al consumo actual".

### Escenario 3 — Manejo de Excepción: Notificación en tiempo real al superar 90%
- **Dado** que la cuota configurada es de `100 GB` y el consumo actual es `89 GB`
- **Cuando** una nueva carga de archivo de `2 GB` se procesa exitosamente elevando el total a `91 GB`
- **Entonces** el dominio emite `OrganizationQuotaThresholdExceededDomainEvent`
- **Y** los administradores conectados reciben una notificación visual de advertencia.

### Escenario 4 — Seguridad y Restricción de Permisos: Aislamiento Multi-Tenant
- **Dado** que un usuario pertenece al Tenant A
- **Cuando** intenta consultar o mutar las cuotas de una organización del Tenant B
- **Entonces** el sistema retorna HTTP 403 Forbidden y la política de RLS impide la visualización de los datos.

---

## 5. Checklist de Preparación para SDD (Handoff)

- [x] Todas las preguntas de negocio fueron respondidas (sin marcadores `[...]` ni dudas pendientes).
- [x] Los roles y permisos (`Quotas.Read`, `Quotas.Update`) corresponden a la matriz PBAC.
- [x] El aislamiento multi-tenant se basa estrictamente en `tenant_id` resuelto desde el JWT.
- [x] Los componentes de UI fueron contrastados con el sistema de diseño atómico en `shared/ui/`.
- [x] La arquitectura respeta Onion puro (inyección de `IRepository` y `IUnitOfWork`, sin DbContext en Application).
- [x] El artefacto está guardado como `specs/PROJ-12_configuracion_limites_almacenamiento/PROJ-12-interview.md`.

> **Siguiente Paso**: Proceder con la habilidad `req-technical-spec-generator` para redactar `PROJ-12.spec.md`, `PROJ-12-plan.md` y `PROJ-12-tasks.md` tomando esta entrevista y la historia de usuario en Jira como base autoritativa.
