# Rule: Redarbor Clean Code & Technical Constraints (`redarbor-clean-code.md`)

All code generated or modified in this repository **MUST** strictly comply with the following rules:

## 1. The 7 Clean Code Rules
1. **100% English Codebase:** All namespaces, classes, interfaces, methods, properties, variables, test names, and comments must be written in English. Only user-facing HTTP/UI response messages may be localized if explicitly requested.
2. **Strict Method Spacing:** Do **NOT** leave blank lines inside method or function bodies, **except** a single blank line immediately before a `return` statement (unless the `return` is the very first line of the block).
3. **Self-Descriptive Method Names:** Method and function names must clearly describe what they do so they do not need explanatory comments.
4. **Single Responsibility & Complexity Limits:**
   - Maximum **25 lines** per method/function body.
   - Maximum **1 level** of nested loops or conditionals (`if` inside `if`, `foreach` inside `if`, etc.). Extract private helpers or use early returns to stay flat.
   - Zero inline comments explaining code sections inside methods.
5. **Environment-Driven Configuration:** All configuration values (database connections, JWT secrets, Redis endpoints, ports, CORS origins) must be read from environment variables / `IConfiguration` (never hardcoded).
6. **One Public Type Per Physical File:** Never place more than one `class`, `enum`, `interface`, or `record` in a single `.cs` or `.ts` file.
7. **Pass Objects Instead of Multiple Primitive Properties:** Prefer cohesive parameter objects/records over long parameter lists (e.g., `User.Create(UserCreationData data)` rather than `User.Create(id, name, email, hash, roleId)`).

## 2. Database & Persistence Invariants
- **Read/Write CQRS Split:**
  - **Reads (`Queries` / `IFinder`):** Must use **Entity Framework Core** with `.AsNoTracking()`.
  - **Writes (`Commands` / `IRepository`):** Must use **Dapper** over `SqlConnection` / `IDbConnectionFactory` for relational SQL Server operations.
- **No Foreign Keys:** Never configure `.HasForeignKey(...)` or `FOREIGN KEY` constraints in SQL Server schemas, EF Core fluent configurations, or migrations. Model references using scalar ID columns and indexes.
