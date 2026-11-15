---
trigger: always_on
---

# Regla: Política Estricta de Commits y Push (`git-commit-policy.md`)

## 1. Prohibición de Commits Automáticos

Queda **estrictamente prohibido** ejecutar `git commit` o `git push` de forma automática durante cualquier workflow o tarea (incluyendo `/project-init`, `/story-authoring`, `/spec-generation`, `/feature-scaffold`, `/bugfix-forensic` o ediciones puntuales).

## 2. Cuándo Sí Está Permitido Hacer Commit / Push

El agente **únicamente** puede ejecutar `git commit` (y `git push` si aplica) cuando se cumpla alguna de estas dos condiciones:

1. **Instrucción explícita del usuario:** El usuario pide directamente hacer commit o push en su mensaje (ej. *"haz commit"*, *"commitea esto"*, *"haz push"*).
2. **Invocación de `/deliver-pr`:** El usuario ejecuta explícitamente el workflow `/deliver-pr` para cerrar la feature, validar la suite global, commitear, hacer push y abrir el Pull Request contra `develop`.

## 3. Comportamiento Esperado

- Al finalizar `/spec-generation`, `/feature-scaffold` o `/bugfix-forensic`, los cambios deben quedar en el working tree de la rama `feature/<KEY>-<slug>` sin commitear, listos para que el usuario los revise o ejecute `/deliver-pr`.
