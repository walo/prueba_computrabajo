---
trigger: always_on
---

# Regla: Confidencialidad Absoluta, Protección de Datos y Tratamiento Local (NDA Compliance)

## 1. Principio Fundamental: Propiedad Intelectual y Sigilo Profesional (NDA)

Todo el código fuente, especificaciones técnicas, esquemas de bases de datos, diagramas, reglas de negocio extraídas de NotebookLM, historias de usuario de Jira y nombres de clientes/proyectos constituyen **Información Confidencial y Secreto Industrial**, amparados bajo Acuerdos de Confidencialidad (NDA) estrictos.

Cualquier filtración o transmisión no autorizada constituye una violación crítica de seguridad y cumplimiento legal.

---

## 2. Directivas Mandatorias de Seguridad y Privacidad

### A. Prohibición Total de Exfiltración a la Nube no Corporativa 🚫
1. Queda terminantemente prohibido subir, transmitir, sincronizar o compartir fragmentos de código fuente, reglas de negocio, esquemas SQL o especificaciones a servicios externos en la nube no autorizados (ej. pastebins, servicios de análisis públicos, foros o APIs de terceros).
2. Todo el código, artefactos técnicos (`specs/`) y base de datos deben residir, compilarse y ejecutarse **exclusivamente en la máquina local del desarrollador** y en los repositorios Git corporativos y privados autorizados explícitamente en `.env`.

### B. Uso Restringido y Anónimo de Búsquedas Web
1. Las herramientas de búsqueda externa en internet (`search_web`, `read_url_content`) están permitidas **únicamente** para consultar documentación técnica pública oficial (ej. documentación de .NET 10, sintaxis de Angular 22, especificaciones RFC 7807, firmas de librerías públicas).
2. **Prohibición de Datos Sensibles en Búsquedas:** Queda estrictamente prohibido incluir en las consultas de búsqueda nombres de clientes, nombres de entidades del dominio de negocio, fragmentos de código propietario o credenciales. Toda consulta técnica debe ser 100% genérica.

### C. Conexiones a Servicios Corporativos Autorizados
Las únicas comunicaciones de red permitidas son:
- **Jira Cloud:** Mediante las credenciales corporativas configuradas en `.env` vía el cliente local seguro (`jira_client.js`).
- **NotebookLM:** A través del MCP oficial configurado localmente.
- **GitHub:** Repositorio remoto corporativo configurado en `git remote origin` vía `github_client.js` o `git push`.
- **Servicios Locales:** Base de datos PostgreSQL y Redis corriendo en contenedores Docker locales (`localhost`).

### D. Protección de Credenciales y Cero Fugas (Zero Leaks)
1. Queda terminantemente prohibido incluir contraseñas reales, tokens API, secrets de JWT o cadenas de conexión en el código fuente, archivos `.md` de especificación, commits de Git o logs.
2. Toda credencial sensible debe residir exclusivamente en `.env` (el cual debe permanecer en `.gitignore`).

---

## 3. Calidad y Cumplimiento

Cualquier propuesta o comando que implique enviar datos privados del proyecto fuera del entorno seguro corporativo será interceptada y rechazada de inmediato.
