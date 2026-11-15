---
trigger: always_on
---

# Rule: Token Optimization, Surgical Context & Concise Communication

## 1. Entrada Quirúrgica (Input Tokens / Graphify First)

1. **Prohibida la Lectura Masiva de Archivos:**
   - Prohibido hacer `cat`, `head` o lectura exhaustiva de múltiples archivos para "entender" la arquitectura.
   - Prohibido inspeccionar árboles de directorios completos a ciegas.

2. **Uso Mandatorio de Graphify Quirúrgico:**
   - Para ubicar dependencias, contratos o firmas, ejecutar primero:
     `graphify query "<componente/método/clase>"`
   - **Prohibidas consultas ultra-genéricas:** No buscar términos amplios como `"User"`, `"class"` o `"service"` que saturan y truncan el contexto. Usar identificadores específicos (ej. `"IUserRepository"`, `"ResetPasswordCommandHandler"`) o relaciones (`graphify path "<Origen>" "<Destino>"`).
   - Leer únicamente el archivo o fragmento exacto que el subgrafo identifique como necesario para la tarea.

3. **Ejecución Focalizada de Tests (TDD Iterativo):**
   - Durante los micro-ciclos Red/Green, **prohibido ejecutar la suite global de tests** de toda la solución para evitar miles de líneas de stdout innecesarias en el contexto.
   - Filtrar exclusivamente por la feature en desarrollo:
     - Backend: `dotnet test --filter FullyQualifiedName~<FeatureName>`
     - Frontend: `cmd.exe /c npm test -- -t "<FeatureName>"`
   - La suite global completa (`dotnet test`, `npm test`) se ejecuta únicamente en la fase de verificación integral y en el workflow de entrega (`deliver-pr`).

## 2. Salida Concisa y Telegráfica (Output Tokens / Brevity Mode)

1. **Cero Relleno Conversacional:**
   - Prohibido usar saludos, cortesías o cierres decorativos (_"¡Hola!", "Con gusto te ayudo"_, etc.).
   - Entrar directo en la acción, causa o solución técnica.

2. **Respuestas Técnicas Directas:**
   - Estructurar reportes con viñetas cortas:
     - **Causa:** 1-2 líneas.
     - **Acción / Cambio:** Lista puntual de modificaciones y rutas de archivos.
     - **Estado:** Pruebas y compilación (Verde / Rojo).

3. **Optimización de Código en Salida:**
   - No reproducir archivos completos en la respuesta; usar diffs mínimos o referencias exactas de líneas.
   - Entregar bloques completos únicamente cuando se cree un archivo nuevo desde cero.

4. **Preservación de Rigor Técnico:**
   - La brevedad no autoriza omitir validaciones de dominio, cobertura de pruebas ni patrones arquitectónicos (Clean Architecture / DDD / Onion Architecture).
