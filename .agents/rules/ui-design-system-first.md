---
trigger: always_on
---

# Regla: Uso Obligatorio del Sistema de Diseño (`@accionadev/ui-kit`)

## 1. Principio Fundamental: Cero UI Nativa Improvisada

Ante **cualquier** solicitud de interfaz de usuario —incluso peticiones simples o informales como *"ponme un botón"*, *"crea una tarjeta"*, *"agrega un ícono"*, *"haz un formulario"*, *"muestra una tabla"* o *"pon un modal"*— el agente **DEBE** consultar primero el Sistema de Diseño corporativo antes de escribir una sola línea de HTML o TypeScript.

---

## 2. Protocolo Obligatorio Antes de Escribir UI

1. **Lectura Obligatoria del Manifiesto:**
   - Leer `../ui-design-system/ui-manifest.json` para verificar el selector exacto (`<ad-*>`) y sus `inputs` / `outputs`.
2. **Importación Única desde `@accionadev/ui-kit`:**
   - Usar exclusivamente los componentes del catálogo oficial (`<ad-button>`, `<ad-input>`, `<ad-select>`, `<ad-badge>`, `<ad-icon>`, `<ad-brand-logo>`, `<ad-card>`, `<ad-alert>`, `<ad-empty-state>`, `<ad-tabs>`, `<ad-breadcrumb>`, `<ad-modal>`, `<ad-table>`, `<ad-app-shell>`).
3. **Prohibiciones Estrictas en `src/frontend/`:**
   - **Prohibido** escribir elementos HTML nativos estilizados a mano para controles que existen en el kit (`<button>`, `<input>`, `<select>`, tarjetas con `<div class="rounded border p-4 shadow">`, tablas `<table>` crudas o modales caseros).
   - **Prohibido** escribir bloques `<svg>` manuales para iconos o logos; usar `<ad-icon>` o `<ad-brand-logo>`.
   - **Prohibido** usar colores `#hex` arbitrarios; usar exclusivamente tokens semánticos (`bg-primary`, `text-foreground`, `bg-surface`, etc.) heredados de `tailwind.preset.js` y `styles.css`.
   - **Prohibido** crear átomos, moléculas u organismos nuevos dentro de `src/frontend/`.
4. **Si Falta un Componente en `ui-manifest.json`:**
   - Pausar la vista de negocio y delegar su creación a `../ui-design-system/.agents/skills/fe-design-system/SKILL.md` (construir en `ui-kit`, exportar en `public-api.ts`, registrar en `ui-manifest.json` y crear página en `showcase`), y luego importarlo desde `'@accionadev/ui-kit'`.

