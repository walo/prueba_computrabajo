---
trigger: always_on
---

# Regla: Grounding de Negocio en NotebookLM y Gestión de Reglas Faltantes

## 1. Cuaderno Oficial del Proyecto

- **Notebook ID:** Configurable en la raíz del proyecto o en el archivo de entorno del agente (actualizar con el ID correspondiente al iniciar un proyecto).
- **Herramienta de Acceso:** MCP NotebookLM (`notebook_query`).

---

## 2. Directivas Mandatorias

### A. Fuente Única de Verdad de Negocio
1. Todas las reglas de negocio, invariantes, estados de entidades, límites de dominio y roles **deben consultarse y extraerse directamente del cuaderno oficial de NotebookLM**.
2. **Prohibición Total de Invención o Suposición:** Queda terminantemente prohibido inventar, deducir o asumir reglas de negocio, fórmulas de cálculo o validaciones no respaldadas formalmente por las fuentes del cuaderno.

### B. Protocolo Mandatorio ante Reglas de Negocio Faltantes o No Contempladas ⚠️
Si durante la redacción de una especificación técnica, el diseño de una feature, una refactorización o la implementación de código, el agente detecta que:
- Existe un caso límite, flujo alternativo o validación que **no está contemplado en el cuaderno**, O
- Hay una contradicción o vacío de negocio en las especificaciones existentes:

> **EL AGENTE DEBE DETENERSE Y PREGUNTAR AL USUARIO.**
> 
> Queda prohibido asumir la regla por conveniencia técnica o "para que el código funcione". El agente debe formular la pregunta puntual al usuario:
> 1. Describir con precisión el escenario o vacío detectado.
> 2. Presentar las opciones de resolución viables.
> 3. Solicitar la confirmación del usuario sobre qué regla adoptar y si debe incorporarse formalmente a la documentación.

### C. Bloqueo por Fallo de Autenticación o Desconexión (Gateway Infranqueable)
Si la consulta a NotebookLM falla, responde con error de sesión expirada (`Authentication expired`) o no responde:
1. **DETENER EL FLUJO DE INMEDIATO.**
2. Prohibido continuar generando historias en Jira, documentos de especificación técnica o código basándose en memoria local o deducciones.
3. Notificar al usuario inmediatamente indicando el error de autenticación y esperar la confirmación de renovación de la sesión (ej. reautenticación en `notebooklm-mcp-auth` o actualización de cookies).

### D. Trazabilidad en Historias y Specs
Toda Historia de Usuario o Spec técnica debe citar los conceptos clave extraídos del cuaderno, garantizando coherencia absoluta entre la definición funcional y el código final.
