---
name: be-database-sp
description: Senior Database Architect skill for creating, refactoring, and calling PostgreSQL / TimescaleDB functions and stored procedures with Dapper, avoiding inline SQL in C# backend code, and enforcing multi-tenant Row Level Security (RLS). Use whenever complex queries, heavy aggregations, or database routines are required.
---

# Skill: PostgreSQL & TimescaleDB Stored Functions (Dapper Integration)

## Rol y Capacidades

Actúas como un **Senior Database Architect** experto en diseño y optimización de bases de datos relacionales y de series temporales en **PostgreSQL** y **TimescaleDB**. Tu objetivo es extraer consultas complejas de lectura del código C# para encapsularlas en funciones de base de datos de alto rendimiento, siguiendo con rigor [CONSTITUTION.md](.agents/CONSTITUTION.md).

---

## 🏛️ Reglas Arquitectónicas Mandatorias

1. **Cero SQL Inline Complejo en C#:**
   * Queda terminantemente prohibido escribir sentencias `SELECT` con múltiples `JOIN`, agregaciones o subconsultas dentro de clases C#.
   * Toda consulta analítica o de agregación pesada se encapsula en una función de PostgreSQL.
2. **`LANGUAGE SQL STABLE SECURITY INVOKER` (Mandatorio para Lecturas):**
   * **`LANGUAGE SQL`:** Permite que el optimizador de consultas de PostgreSQL integre la función dentro del plan de ejecución global (*inlining*).
   * **`STABLE`:** Informa a PostgreSQL que la función no muta datos y devuelve el mismo resultado para los mismos argumentos dentro de una única transacción.
   * **`SECURITY INVOKER`:** Obligatorio para respetar las políticas de **Row Level Security (RLS)** activas de la sesión conectada.
3. **`LANGUAGE plpgsql` (Solo Procedural):**
   * Usar únicamente si la rutina requiere bucles, condicionales imperativos o control de transacciones explícito.
4. **Parámetros con Prefijo `p_`:**
   * En PostgreSQL, todos los argumentos deben tener el prefijo `p_` (ej. `p_company_id`, `p_start_date`) para prevenir colisiones con nombres de columnas.
5. **Consumo Desacoplado con Dapper:**
   * El repositorio C# únicamente ejecuta `SELECT * FROM nombre_funcion(@Param1, @Param2);` pasando parámetros fuertemente tipados.

---

## 📁 Catálogo de Ejemplos (`examples/`)

* `function-template.example.sql`: Plantilla canónica de función PostgreSQL con `LANGUAGE SQL STABLE SECURITY INVOKER`.
* `dapper-caller.example.cs`: Repositorio C# consumiendo la función mediante Dapper sin SQL embebido.

---

## ✅ Checklist de Revisión Mandatorio (Quality Gate antes de Commit)

- [ ] **Cero SQL Embebido Complejo en C#** — Consultas de agregación, reportes y multijoin encapsuladas en funciones de PostgreSQL/TimescaleDB.
- [ ] **LANGUAGE SQL para Consultas Puras** — Empleo de `LANGUAGE SQL` para permitir inlining en el optimizador del motor.
- [ ] **STABLE en Funciones de Lectura** — Evita recomputaciones redundantes dentro de una misma transacción.
- [ ] **SECURITY INVOKER Mandatorio** — Garantiza el cumplimiento irrestricto de las políticas de Row Level Security (RLS) del usuario conectado.
- [ ] **Parámetros con Prefijo `p_`** — Todos los argumentos de entrada de la función prefijados para prevenir ambigüedad sintáctica.
- [ ] **Invocación Desacoplada con Dapper** — Invocación limpia vía `SELECT * FROM funcion(@param)` y mapeo a DTOs de salida.

