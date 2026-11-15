<!-- EJEMPLO DE REFERENCIA — no es una historia del backlog local.
     Muestra el nivel de detalle y estructura Gherkin 100% de negocio que se publica en Jira Cloud.
     PROHIBIDO incluir detalles técnicos (endpoints, tablas, frameworks, clases o códigos HTTP).
     El código PROJ-12 aquí es de referencia: Jira asigna el Issue Key automáticamente al crear. -->

# Historia de Usuario: Configuración de Límites de Almacenamiento por Organización

**Jira Issue**: PROJ-12  
**Prioridad**: Alta  
**Fecha**: 2026-10-23  
**Origen**: Requerimiento funcional del negocio  

---

## 1. Definición (Como / Quiero / Para)

- **Como**: Administrador de Organización
- **Quiero**: Definir una cuota máxima de almacenamiento y recibir una notificación cuando una sede o departamento alcance el 90% de su capacidad
- **Para**: Evitar bloqueos operativos y gestionar oportunamente la ampliación de recursos de mi empresa

---

## 2. Contexto y Reglas de Negocio

El límite de cuota es una capacidad configurable por organización o departamento. El control de consumo garantiza que cada empresa opere dentro de su capacidad contratada y que sus administradores solo puedan consultar y gestionar la información perteneciente a su propia organización, sin visibilidad sobre otras empresas registradas en la plataforma.

### Reglas de Negocio:
- **Regla 1 (Capacidad mínima):** No se permite configurar una cuota máxima inferior al espacio que la organización ya tiene consumido actualmente.
- **Regla 2 (Umbral de alerta preventiva):** Al alcanzar o superar el 90% de la cuota configurada, el sistema debe emitir una alerta preventiva visible para el administrador.
- **Regla 3 (Límite estricto del 100%):** Si el consumo alcanza el 100% de la capacidad asignada, no se permitirán nuevas cargas de archivos hasta que se libere espacio o se amplíe la cuota.
- **Regla 4 (Privacidad entre organizaciones):** Un administrador únicamente puede visualizar y modificar los límites de su propia organización.

---

## 3. Criterios de Aceptación (Gherkin)

### Escenario 1 — Configuración exitosa de la cuota (Camino feliz)
- **Dado** que un Administrador de Organización ha iniciado sesión en el panel de configuración de su empresa
- **Cuando** establece el límite de cuota de almacenamiento en 500 GB y confirma la acción de guardar
- **Entonces** el sistema registra el nuevo límite asignado a la organización
- **Y** muestra un mensaje confirmando que la cuota fue actualizada exitosamente

### Escenario 2 — Consumo que supera el umbral preventivo del 90% (Caso alternativo)
- **Dado** que una organización tiene configurada una cuota máxima de 500 GB
- **Cuando** un integrante de la organización sube un archivo que eleva el espacio utilizado al 91% (455 GB)
- **Entonces** el sistema genera una alerta preventiva de proximidad al límite de capacidad
- **Y** el panel del administrador muestra un aviso destacado indicando que la organización ha superado el 90% de su almacenamiento

### Escenario 3 — Intento de asignar una cuota menor al espacio ya utilizado (Rechazo controlado)
- **Dado** que una organización tiene actualmente 120 GB de almacenamiento en uso
- **Cuando** el Administrador de Organización intenta configurar la nueva cuota máxima en 100 GB
- **Entonces** el sistema rechaza el cambio
- **Y** informa al administrador que la nueva cuota no puede ser inferior al espacio actualmente utilizado (120 GB)
- **Y** mantiene sin cambios el límite que estaba vigente

### Escenario 4 — Restricción de acceso a otra organización (Seguridad de negocio)
- **Dado** un Administrador perteneciente a la "Empresa A"
- **Cuando** intenta consultar o modificar la cuota de almacenamiento perteneciente a la "Empresa B"
- **Entonces** el sistema deniega el acceso a la operación
- **Y** muestra un aviso indicando que no cuenta con permisos sobre esa organización

---

## 4. Límites Operativos y Condiciones de Uso

- Las notificaciones por superación del umbral del 90% no deben repetirse más de una vez cada 15 minutos para un mismo departamento, evitando saturar de alertas al administrador.
- Cualquier modificación en la cuota de almacenamiento debe quedar registrada en el historial de actividad indicando quién realizó el cambio, la fecha y el valor anterior frente al nuevo valor.
