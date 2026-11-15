# Graph Report - computrabajo  (2026-11-14)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 1750 nodes · 3211 edges · 123 communities (107 shown, 16 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 151 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- LogoutCommand
- Application
- CreateItemCommand
- new-story.mjs
- GetUsersQuery
- query-and-finder.example.cs
- rxjs
- AuthTokensResponse
- auth.service.ts
- @angular/core
- github_client.js
- CacheService
- 20261115031703_InitialAuthMigration.Designer.cs
- .SeedAsync
- mediatr
- shell.component.ts
- .InvokeAsync
- IAuthFinder
- ITokenBlacklistService
- service-http.example.ts
- app.config.ts
- webapi-controllers.example.cs
- package.json
- User
- RefreshToken
- CacheService
- .RotateRefreshTokenAsync
- security-middleware-test.example.cs
- NotificationService
- GetCurrentUserQuery
- microsoft_extensions_dependencyinjection
- Application.Common.Interfaces
- .AddPersistenceInfrastructure
- FeatureSummaryReportDto
- domain-entities.example.cs
- app-routes.example.ts
- @angular/router
- JwtTokenRequest
- schematics
- .AddAuthenticationInfrastructure
- Entity
- IDomainEvent
- Application.Features.Auth.DTOs
- http
- microsoft_entityframeworkcore
- program.example.cs
- TokenBlacklistService
- .Handle
- devDependencies
- token.interceptor.ts
- User
- AppDbContext
- IDomainEvent
- .AddPersistenceInfrastructure
- ValueObject
- Domain.Entities
- RefreshTokenCreationData
- RefreshToken
- Permissions
- error-mapper.ts
- ValidationBehavior
- AuthService
- NotificationService
- UserIdentityDto
- UserRepository
- Role
- .GetPermissionsForUserAsync
- dependencies
- ICurrentUserContext
- .Handle
- application-behaviors.example.cs
- PermissionCacheService
- .GetActiveByUserIdAsync
- IRepository
- .Handle_Should_RotateTokens_When_RefreshTokenIsActive
- AppDbContext
- RefreshTokenRepository
- StorageService
- Role
- AuthorizationMiddlewareTests
- .GetPermissionsForUserAsync
- GlobalExceptionHandlingMiddleware
- build
- .AddJwtAuthentication
- RolePermission
- UserCreationData
- error-handler.service.example.ts
- ShellComponent
- .CheckHealthAsync
- SwaggerExtensions.cs
- GeneratedRefreshToken
- Permission
- IAuditableEntity
- LoginCommandValidatorTests
- angular.json
- scripts
- IAuditableEntity
- feature-routes.example.ts
- FeatureListComponent
- LoaderService
- ICurrentUserContext
- .AddCorsInfrastructure
- error-mapper.example.ts
- modal-shell.component.spec.ts
- .SaveChangesAsync
- EnvironmentLoader
- serve
- development
- production
- frontend
- get_feature_summary_report
- loader.component.example.ts
- main.example.ts
- IPasswordHasher
- api-response.model.example.ts
- permissions.constants.example.ts
- api-response.model.ts
- environment.example.ts
- jest-preset-angular
- environment.prod.ts

## God Nodes (most connected - your core abstractions)
1. `Application.Common.Interfaces` - 57 edges
2. `@angular/core` - 50 edges
3. `RefreshToken` - 41 edges
4. `Domain.Entities` - 31 edges
5. `Domain.Primitives` - 30 edges
6. `@angular/router` - 29 edges
7. `User` - 26 edges
8. `AuthService` - 21 edges
9. `Application.Features.Auth.DTOs` - 21 edges
10. `Entity` - 20 edges

## Surprising Connections (you probably didn't know these)
- `UsersController` --inherits--> `ApiController`  [EXTRACTED]
  src/backend/src/Presentation/Web.API/Controllers/UsersController.cs → .agents/skills/be-dotnet-scaffold/examples/cascaron/webapi-controllers.example.cs
- `AuthController` --references--> `ICurrentUserContext`  [EXTRACTED]
  src/backend/src/Presentation/Web.API/Controllers/AuthController.cs → .agents/skills/be-dotnet-scaffold/examples/cascaron/application-abstractions.example.cs
- `DatabaseSeedRequest` --references--> `IPasswordHasher`  [EXTRACTED]
  src/backend/src/Infrastructure/Persistence/Seeding/DatabaseSeedRequest.cs → .agents/skills/be-dotnet-scaffold/examples/cascaron/application-abstractions.example.cs
- `PermissionAuthorizationMiddlewareTests` --references--> `IPermissionCacheService`  [EXTRACTED]
  src/backend/tests/Tests.UnitTests/Presentation/Middlewares/PermissionAuthorizationMiddlewareTests.cs → .agents/skills/be-dotnet-scaffold/examples/cascaron/application-abstractions.example.cs
- `LoginCommandHandler` --references--> `IAuthFinder`  [EXTRACTED]
  src/backend/src/Core/Application/Features/Auth/Commands/Login/LoginCommandHandler.cs → .agents/skills/be-dotnet-scaffold/examples/cascaron/application-abstractions.example.cs

## Import Cycles
- None detected.

## Communities (123 total, 16 thin omitted)

### Community 0 - "LogoutCommand"
Cohesion: 0.06
Nodes (42): AllowAnonymous, Authorize, CancellationToken, Error, HasPermission, HttpGet, HttpPost, IActionResult (+34 more)

### Community 1 - "Application"
Cohesion: 0.06
Nodes (53): BCrypt.Net-Next (4.2.0), Dapper (2.1.66), ErrorOr (2.1.1), FluentValidation (12.2.0), FluentValidation.DependencyInjectionExtensions (12.2.0), MediatR (12.4.1), MediatR.Contracts (2.0.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.12) (+45 more)

### Community 2 - "CreateItemCommand"
Cohesion: 0.08
Nodes (31): CreateItemCommand, CreateItemCommandHandler, CancellationToken, ErrorOr, Guid, ICompanyRepository, IItemDomainService, IItemRepository (+23 more)

### Community 3 - "new-story.mjs"
Cohesion: 0.10
Nodes (36): ambar(), cian(), comandoCheck(), comandoCreateJira(), comandoList(), comandoNew(), entradasSpecs(), listarCarpetasSpecs() (+28 more)

### Community 4 - "GetUsersQuery"
Cohesion: 0.07
Nodes (29): Application.Features.Users.DTOs, IUserFinder, CancellationToken, IReadOnlyList, Task, UserSummaryResponse, DateTime, Guid (+21 more)

### Community 5 - "query-and-finder.example.cs"
Cohesion: 0.09
Nodes (31): CancellationToken, HttpGet, HttpPost, IActionResult, ProducesResponseType, Task, ItemsController, GetItemsQuery (+23 more)

### Community 6 - "rxjs"
Cohesion: 0.07
Nodes (22): appConfig, loaderInterceptor(), USE_GLOBAL_LOADER, StorageService, Injectable, addBearerHeader(), handleUnauthorizedWithSilentRefresh(), isPublicAuthEndpoint() (+14 more)

### Community 7 - "AuthTokensResponse"
Cohesion: 0.12
Nodes (20): Program, Fact, HttpClient, Task, CustomWebApplicationFactory, IntegrationTestBase, UsersEndpointsIntegrationTests, IClassFixture (+12 more)

### Community 8 - "auth.service.ts"
Cohesion: 0.12
Nodes (16): c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adalertcomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adbrandlogocomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adbuttoncomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adinputcomponent, ref_angular_common_http_testing, ref_angular_core_testing, AUTH_ERROR_CATALOG, API_PATHS (+8 more)

### Community 9 - "@angular/core"
Cohesion: 0.09
Nodes (19): DashboardComponent, Component, AUTH_ERRORS, LoginComponent, Component, StatusPageComponent, Component, ToastContainerComponent (+11 more)

### Community 10 - "github_client.js"
Cohesion: 0.11
Nodes (25): createPR(), envVars, { execSync }, fs, getCurrentBranch(), getRepoInfo(), githubRequest(), https (+17 more)

### Community 11 - "CacheService"
Cohesion: 0.11
Nodes (18): CancellationToken, IConnectionMultiplexer, IDatabase, IMemoryCache, Task, TimeSpan, CacheService, Infrastructure.Shared.Services (+10 more)

### Community 12 - "20261115031703_InitialAuthMigration.Designer.cs"
Cohesion: 0.10
Nodes (19): Infrastructure.Persistence.Migrations, microsoft_entityframeworkcore_infrastructure, microsoft_entityframeworkcore_metadata, microsoft_entityframeworkcore_migrations, microsoft_entityframeworkcore_storage_valueconversion, Migration, MigrationBuilder, ModelSnapshot (+11 more)

### Community 13 - ".SeedAsync"
Cohesion: 0.17
Nodes (15): ILoggerFactory, Guid, RolePermissionCreationData, AppDbContext, ILogger, DatabaseSeedRequest, AppDbContext, CancellationToken (+7 more)

### Community 14 - "mediatr"
Cohesion: 0.09
Nodes (16): AbstractValidator, Application.Features.Auth.Events, Application.Common.Behaviors, Application.Features.Auth.Commands.RefreshToken, fluentvalidation, mediatr, ValidationBehavior, CancellationToken (+8 more)

### Community 15 - "shell.component.ts"
Cohesion: 0.13
Nodes (15): c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adappshellcomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adappshellnavitem, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adbadgecomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adcardcomponent, ext_ui_design_system_projects_ui_kit_src_public_api_ts, ngx-permissions, src_frontend_node_modules_angular_core_types_rxjs_interop_d, src_frontend_node_modules_angular_core_types_rxjs_interop_d_tosignal (+7 more)

### Community 16 - ".InvokeAsync"
Cohesion: 0.15
Nodes (17): AuthorizeAttribute, HasPermissionAttribute, Permission, CancellationToken, Guid, HttpContext, IReadOnlyList, RequestDelegate (+9 more)

### Community 17 - "IAuthFinder"
Cohesion: 0.21
Nodes (10): IAuthFinder, ICacheService, IPermissionCacheService, IRefreshTokenRepository, CancellationToken, Guid, HashSet, IReadOnlyList (+2 more)

### Community 18 - "ITokenBlacklistService"
Cohesion: 0.15
Nodes (15): ITokenBlacklistService, HttpContext, RequestDelegate, Task, JwtBlacklistMiddleware, HttpContext, RequestDelegate, Task (+7 more)

### Community 19 - "service-http.example.ts"
Cohesion: 0.10
Nodes (12): CreateFeatureDto, FeatureDto, FeatureHttpService, Injectable, FeatureFilter, FeatureSignalStore, Injectable, ref_core_models (+4 more)

### Community 20 - "app.config.ts"
Cohesion: 0.14
Nodes (13): c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adiconcomponent, ref_angular_common_http, App, appConfig, routes, Component, loaderInterceptor(), LoaderService (+5 more)

### Community 21 - "webapi-controllers.example.cs"
Cohesion: 0.16
Nodes (13): application_attributes, application_features_items_queries_getitems, Application.Features.Items.DTOs, Web.API.Attributes, Application.Features.Auth.Queries.GetCurrentUser, Domain.Enums, Web.API.Controllers, Application.Features.Users.Queries.GetUsers (+5 more)

### Community 22 - "package.json"
Cohesion: 0.10
Nodes (20): @angular/build, @angular/cli, @angular/compiler, @angular/compiler-cli, autoprefixer, jest, jest-environment-jsdom, jsdom (+12 more)

### Community 23 - "User"
Cohesion: 0.12
Nodes (14): IUserRepository, CancellationToken, Task, DateTime, Guid, User, CreatedAtUtc, Email (+6 more)

### Community 24 - "RefreshToken"
Cohesion: 0.12
Nodes (16): EntityTypeBuilder, RefreshTokenConfiguration, IEntityTypeConfiguration, DateTime, Guid, RefreshToken, CreatedAtUtc, ExpiresAtUtc (+8 more)

### Community 25 - "CacheService"
Cohesion: 0.16
Nodes (11): CacheSetRequest, TimeSpan, ICacheService, CancellationToken, Task, CancellationToken, IConnectionMultiplexer, IDatabase (+3 more)

### Community 26 - ".RotateRefreshTokenAsync"
Cohesion: 0.19
Nodes (11): IJwtProvider, AccessTokenMinutes, DateTime, CancellationToken, Task, IUnitOfWork, RefreshTokenCommandHandler, CancellationToken (+3 more)

### Community 27 - "security-middleware-test.example.cs"
Cohesion: 0.13
Nodes (12): application_abstractions_caching, Tests.UnitTests.Core.Application.Features.Auth.Commands.RefreshToken, Application.Features.Auth.Commands.Login, Tests.UnitTests.Presentation.Middlewares, Tests.UnitTests.Core.Application.Features.Auth.Commands.Logout, Tests.WebApi.Middlewares, Tests.UnitTests.Core.Application.Features.Auth.Commands.Login, fluentassertions (+4 more)

### Community 28 - "NotificationService"
Cohesion: 0.18
Nodes (9): c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adtoast, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adtoastcomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_toastvariant, ToastMessage, ToastVariant, NotificationService, Injectable, ToastContainerComponent (+1 more)

### Community 29 - "GetCurrentUserQuery"
Cohesion: 0.19
Nodes (13): GetCurrentUserQuery, GetCurrentUserQueryHandler, CancellationToken, ErrorOr, Guid, Task, CurrentUserResponse, Guid (+5 more)

### Community 30 - "microsoft_extensions_dependencyinjection"
Cohesion: 0.14
Nodes (11): IConfiguration, IServiceCollection, CorsExtensions, Infrastructure.Shared.Caching, microsoft_extensions_caching_memory, microsoft_extensions_dependencyinjection, IConfiguration, IServiceCollection (+3 more)

### Community 31 - "Application.Common.Interfaces"
Cohesion: 0.23
Nodes (8): Application.Common.Interfaces, Infrastructure.Authentication, microsoft_extensions_configuration, microsoft_identitymodel_tokens, system_identitymodel_tokens_jwt, system_security_claims, system_security_cryptography, system_text

### Community 32 - ".AddPersistenceInfrastructure"
Cohesion: 0.13
Nodes (12): Infrastructure.Persistence.Connections, microsoft_data_sqlclient, IDbConnectionFactory, IConfiguration, ConnectionStringResolver, IConfiguration, IDbConnection, SqlConnectionFactory (+4 more)

### Community 33 - "FeatureSummaryReportDto"
Cohesion: 0.19
Nodes (14): CancellationToken, DateTime, Guid, IDbConnection, IEnumerable, Task, FeatureReportRepository, FeatureSummaryReportDto (+6 more)

### Community 34 - "domain-entities.example.cs"
Cohesion: 0.13
Nodes (15): Role, Permission, Code, Name, RolePermission, Permission, PermissionId, RoleId (+7 more)

### Community 35 - "app-routes.example.ts"
Cohesion: 0.12
Nodes (14): routes, ./auth/auth.routes, ./dashboard/dashboard.component, ./features/features.routes, FEATURES_ROUTES, authGuard(), permissionGuard(), publicGuard() (+6 more)

### Community 36 - "@angular/router"
Cohesion: 0.18
Nodes (8): c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_adstatuspagecomponent, c_proyectos_prueba_computrabajo_ui_design_system_projects_ui_kit_src_public_api_statuscode, @angular/router, AUTH_ROUTES, authGuard(), publicGuard(), StatusPageComponent, Component

### Community 37 - "JwtTokenRequest"
Cohesion: 0.15
Nodes (9): Claim, SigningCredentials, GeneratedAccessToken, DateTime, JwtTokenRequest, Guid, IConfiguration, JwtProvider (+1 more)

### Community 38 - "schematics"
Cohesion: 0.12
Nodes (17): schematics, skipTests, skipTests, skipTests, skipTests, skipTests, skipTests, skipTests (+9 more)

### Community 39 - ".AddAuthenticationInfrastructure"
Cohesion: 0.16
Nodes (8): IPasswordHasher, DateTime, IConfiguration, JwtProvider, AccessTokenMinutes, IServiceCollection, DependencyInjection, PasswordHasher

### Community 40 - "Entity"
Cohesion: 0.14
Nodes (10): IReadOnlyCollection, List, AggregateRoot, Entity, DomainEvents, Id, IEquatable, AggregateRoot (+2 more)

### Community 41 - "IDomainEvent"
Cohesion: 0.13
Nodes (11): Guid, IDomainEvent, EventId, OccurredOnUtc, Domain.Events, Guid, RolePermissionsChangedDomainEvent, IReadOnlyCollection (+3 more)

### Community 42 - "Application.Features.Auth.DTOs"
Cohesion: 0.19
Nodes (9): Tests.IntegrationTests.Examples, Tests.IntegrationTests.Features.Auth, Tests.IntegrationTests.Common, Application.Features.Auth.DTOs, microsoft_aspnetcore_hosting, microsoft_aspnetcore_mvc_testing, system_net, system_net_http_headers (+1 more)

### Community 43 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 44 - "microsoft_entityframeworkcore"
Cohesion: 0.26
Nodes (6): Infrastructure.Persistence.Repositories, Infrastructure.Persistence.Finders, Infrastructure.Persistence.Configurations, dapper, microsoft_entityframeworkcore, microsoft_entityframeworkcore_metadata_builders

### Community 45 - "program.example.cs"
Cohesion: 0.22
Nodes (11): Infrastructure.Persistence, Web.API.Infrastructure, Web.API.Services, Infrastructure.Shared, Web.API.HealthChecks, Web.API.Middlewares, Infrastructure.ExternalServices, Application (+3 more)

### Community 46 - "TokenBlacklistService"
Cohesion: 0.19
Nodes (8): BlacklistTokenRequest, DateTime, ITokenBlacklistService, CancellationToken, Task, CancellationToken, Task, TokenBlacklistService

### Community 47 - ".Handle"
Cohesion: 0.28
Nodes (10): LoginCommand, LoginCommandHandler, CancellationToken, ErrorOr, Task, CancellationToken, Fact, Mock (+2 more)

### Community 48 - "devDependencies"
Cohesion: 0.13
Nodes (15): devDependencies, @angular/build, @angular/cli, @angular/compiler-cli, autoprefixer, jest, jest-environment-jsdom, jest-preset-angular (+7 more)

### Community 49 - "token.interceptor.ts"
Cohesion: 0.22
Nodes (9): src_frontend_src_app_core_config_error_messages, attachBearerToken(), isPublicAuthEndpoint(), notifyHttpError(), retryWithRefresh(), tokenInterceptor(), ErrorHandlerService, Injectable (+1 more)

### Community 50 - "User"
Cohesion: 0.15
Nodes (12): DateTime, Guid, User, CompanyId, CreatedAt, Email, IsActive, Name (+4 more)

### Community 51 - "AppDbContext"
Cohesion: 0.14
Nodes (11): DbContextOptions, DbSet, ModelBuilder, AppDbContext, Permissions, RefreshTokens, Roles, Users (+3 more)

### Community 52 - "IDomainEvent"
Cohesion: 0.15
Nodes (12): INotification, INotificationHandler, RolePermissionsChangedEventHandler, CancellationToken, Task, RolePermissionsChangedNotification, Guid, DateTime (+4 more)

### Community 53 - ".AddPersistenceInfrastructure"
Cohesion: 0.24
Nodes (9): IConfiguration, IServiceCollection, DependencyInjection, AppDbContext, CancellationToken, Guid, IReadOnlyList, Task (+1 more)

### Community 54 - "ValueObject"
Cohesion: 0.22
Nodes (4): IEnumerable, ValueObject, IEnumerable, ValueObject

### Community 55 - "Domain.Entities"
Cohesion: 0.24
Nodes (4): Infrastructure.Persistence.Infrastructure.Persistence.Configurations, Domain.Entities, Domain.Primitives, Tests.UnitTests.Core.Domain.Entities

### Community 56 - "RefreshTokenCreationData"
Cohesion: 0.26
Nodes (7): Guid, RefreshTokenCreationData, CancellationToken, Fact, Task, Fact, RefreshTokenTests

### Community 57 - "RefreshToken"
Cohesion: 0.17
Nodes (10): RefreshToken, CreatedAt, ExpiresAt, Id, IsActive, IsUsed, ReplacedByTokenHash, RevokedAt (+2 more)

### Community 58 - "Permissions"
Cohesion: 0.17
Nodes (8): IReadOnlyList, Categories, InventoryMovements, Permissions, Products, Roles, Users, PermissionCreationData

### Community 59 - "error-mapper.ts"
Cohesion: 0.23
Nodes (7): LoginComponent, Component, ProblemDetails, DEFAULT_FALLBACK_MESSAGE, ErrorCatalog, extractProblemMessage(), mapHttpErrorToMessage()

### Community 60 - "ValidationBehavior"
Cohesion: 0.24
Nodes (9): LoggingBehavior, ValidationBehavior, CancellationToken, IEnumerable, ILogger, IValidator, RequestHandlerDelegate, Task (+1 more)

### Community 62 - "NotificationService"
Cohesion: 0.27
Nodes (4): NotificationService, ToastMessage, ToastType, Injectable

### Community 63 - "UserIdentityDto"
Cohesion: 0.31
Nodes (7): IAuthFinder, CancellationToken, Guid, IReadOnlyList, Task, UserIdentityDto, Guid

### Community 64 - "UserRepository"
Cohesion: 0.33
Nodes (6): IDbConnection, AppDbContext, CancellationToken, Guid, Task, UserRepository

### Community 65 - "Role"
Cohesion: 0.20
Nodes (8): Guid, Role, Code, Description, Name, RoleCreationData, EntityTypeBuilder, RoleConfiguration

### Community 66 - ".GetPermissionsForUserAsync"
Cohesion: 0.36
Nodes (7): CancellationToken, Guid, HashSet, List, Task, TimeSpan, PermissionCacheService

### Community 67 - "dependencies"
Cohesion: 0.18
Nodes (11): dependencies, @angular/common, @angular/compiler, @angular/core, @angular/forms, @angular/platform-browser, @angular/router, lucide (+3 more)

### Community 68 - "ICurrentUserContext"
Cohesion: 0.20
Nodes (10): ICurrentUserContext, Email, IsAuthenticated, UserId, IHttpContextAccessor, Guid, CurrentUserContext, Email (+2 more)

### Community 69 - ".Handle"
Cohesion: 0.27
Nodes (7): LogoutCommandHandler, CancellationToken, ErrorOr, Success, Task, Mock, LogoutCommandHandlerTests

### Community 70 - "application-behaviors.example.cs"
Cohesion: 0.22
Nodes (6): Application.Behaviors, Infrastructure.Persistence.Seeding, Domain.Constants, microsoft_extensions_logging, TokenTypes, system_diagnostics

### Community 71 - "PermissionCacheService"
Cohesion: 0.36
Nodes (7): CancellationToken, Guid, HashSet, List, Task, TimeSpan, PermissionCacheService

### Community 72 - ".GetActiveByUserIdAsync"
Cohesion: 0.33
Nodes (5): IRefreshTokenRepository, CancellationToken, Guid, IReadOnlyList, Task

### Community 73 - "IRepository"
Cohesion: 0.33
Nodes (5): IUserRepository, Guid, CancellationToken, Task, IRepository

### Community 74 - ".Handle_Should_RotateTokens_When_RefreshTokenIsActive"
Cohesion: 0.31
Nodes (7): RefreshTokenCommand, ErrorOr, CancellationToken, Fact, Mock, Task, RefreshTokenCommandHandlerTests

### Community 75 - "AppDbContext"
Cohesion: 0.20
Nodes (9): DbContextOptions, DbSet, ModelBuilder, AppDbContext, Permissions, RefreshTokens, RolePermissions, Roles (+1 more)

### Community 76 - "RefreshTokenRepository"
Cohesion: 0.33
Nodes (6): AppDbContext, CancellationToken, Guid, IReadOnlyList, Task, RefreshTokenRepository

### Community 78 - "Role"
Cohesion: 0.22
Nodes (8): RolePermission, Role, Role, Code, Description, Name, RolePermissions, ICollection

### Community 79 - "AuthorizationMiddlewareTests"
Cohesion: 0.31
Nodes (7): CancellationToken, Fact, IPermissionCacheService, Mock, Task, AuthorizationMiddlewareTests, AuthorizationMiddleware

### Community 80 - ".GetPermissionsForUserAsync"
Cohesion: 0.39
Nodes (5): IPermissionCacheService, CancellationToken, Guid, HashSet, Task

### Community 81 - "GlobalExceptionHandlingMiddleware"
Cohesion: 0.39
Nodes (6): HttpContext, ILogger, RequestDelegate, Task, GlobalExceptionHandlingMiddleware, ValidationException

### Community 82 - "build"
Cohesion: 0.22
Nodes (9): build, builder, configurations, defaultConfiguration, options, assets, browser, styles (+1 more)

### Community 83 - ".AddJwtAuthentication"
Cohesion: 0.39
Nodes (5): JwtBearerOptions, IConfiguration, IServiceCollection, JwtExtensions, TokenValidationParameters

### Community 84 - "RolePermission"
Cohesion: 0.29
Nodes (6): Guid, RolePermission, PermissionId, RoleId, EntityTypeBuilder, RolePermissionConfiguration

### Community 85 - "UserCreationData"
Cohesion: 0.39
Nodes (4): Guid, UserCreationData, Fact, UserTests

### Community 86 - "error-handler.service.example.ts"
Cohesion: 0.29
Nodes (5): ErrorHandlerService, Injectable, UiErrorMessage, RFC-7807, ref_config_error_messages_json

### Community 88 - ".CheckHealthAsync"
Cohesion: 0.29
Nodes (6): HealthCheckContext, HealthCheckResult, IHealthCheck, CancellationToken, Task, DatabaseHealthCheck

### Community 89 - "SwaggerExtensions.cs"
Cohesion: 0.33
Nodes (4): microsoft_openapi_models, OpenApiSecurityScheme, IServiceCollection, SwaggerExtensions

### Community 90 - "GeneratedRefreshToken"
Cohesion: 0.29
Nodes (3): GeneratedRefreshToken, IJwtProvider, AccessTokenMinutes

### Community 91 - "Permission"
Cohesion: 0.33
Nodes (6): Guid, Permission, Code, Name, EntityTypeBuilder, PermissionConfiguration

### Community 92 - "IAuditableEntity"
Cohesion: 0.29
Nodes (6): DateTime, IAuditableEntity, CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy

### Community 93 - "LoginCommandValidatorTests"
Cohesion: 0.33
Nodes (4): Fact, InlineData, Theory, LoginCommandValidatorTests

### Community 94 - "angular.json"
Cohesion: 0.29
Nodes (6): cli, packageManager, newProjectRoot, projects, $schema, version

### Community 95 - "scripts"
Cohesion: 0.29
Nodes (7): scripts, build, ng, start, test, test:ci, watch

### Community 96 - "IAuditableEntity"
Cohesion: 0.33
Nodes (6): DateTime, IAuditableEntity, CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy

### Community 97 - "feature-routes.example.ts"
Cohesion: 0.33
Nodes (5): ../components/feature-detail.component, ../components/feature-list.component, FEATURE_ROUTES, ref_core_constants, ref_core_guards

### Community 100 - "ICurrentUserContext"
Cohesion: 0.33
Nodes (5): ICurrentUserContext, Email, IsAuthenticated, UserId, Guid

### Community 101 - ".AddCorsInfrastructure"
Cohesion: 0.47
Nodes (3): IConfiguration, IServiceCollection, CorsExtensions

### Community 102 - "error-mapper.example.ts"
Cohesion: 0.40
Nodes (3): DEFAULT_FALLBACK_MESSAGE, ErrorCatalog, ProblemDetails

### Community 103 - "modal-shell.component.spec.ts"
Cohesion: 0.40
Nodes (3): TestHostComponent, Component, ref_modal_shell_organism_example

### Community 104 - ".SaveChangesAsync"
Cohesion: 0.40
Nodes (3): CancellationToken, Task, IUnitOfWork

### Community 106 - "serve"
Cohesion: 0.40
Nodes (5): serve, architect, builder, configurations, defaultConfiguration

### Community 107 - "development"
Cohesion: 0.40
Nodes (5): development, buildTarget, extractLicenses, optimization, sourceMap

### Community 108 - "production"
Cohesion: 0.40
Nodes (5): production, budgets, buildTarget, fileReplacements, outputHashing

### Community 109 - "frontend"
Cohesion: 0.40
Nodes (5): prefix, projectType, root, sourceRoot, frontend

### Community 110 - "get_feature_summary_report"
Cohesion: 0.50
Nodes (3): get_feature_summary_report(), items, transactions

### Community 111 - "loader.component.example.ts"
Cohesion: 0.50
Nodes (3): GlobalLoaderComponent, Component, ref_core_services_loader_service

### Community 112 - "main.example.ts"
Cohesion: 0.50
Nodes (3): @angular/platform-browser, ref_app_app, ref_app_app_config

## Knowledge Gaps
- **314 isolated node(s):** `ErrorCatalog`, `ProblemDetails`, `ApiError`, `ApiResponse`, `PermissionKey` (+309 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 695 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **16 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Application.Common.Interfaces` connect `Application.Common.Interfaces` to `GetUsersQuery`, `CacheService`, `mediatr`, `IAuthFinder`, `webapi-controllers.example.cs`, `CacheService`, `.RotateRefreshTokenAsync`, `security-middleware-test.example.cs`, `GetCurrentUserQuery`, `microsoft_extensions_dependencyinjection`, `.AddPersistenceInfrastructure`, `JwtTokenRequest`, `Application.Features.Auth.DTOs`, `microsoft_entityframeworkcore`, `program.example.cs`, `TokenBlacklistService`, `.Handle`, `Domain.Entities`, `.Handle`, `application-behaviors.example.cs`, `.GetActiveByUserIdAsync`, `.GetPermissionsForUserAsync`, `GeneratedRefreshToken`, `ICurrentUserContext`, `IPasswordHasher`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **Why does `Domain.Primitives` connect `Domain.Entities` to `CreateItemCommand`, `domain-entities.example.cs`, `.Handle`, `Entity`, `IDomainEvent`, `IRepository`, `.SaveChangesAsync`, `microsoft_entityframeworkcore`, `.Handle`, `IAuthFinder`, `IDomainEvent`, `ValueObject`, `.RotateRefreshTokenAsync`, `security-middleware-test.example.cs`, `IAuditableEntity`, `GetCurrentUserQuery`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Why does `RefreshToken` connect `RefreshToken` to `.GetActiveByUserIdAsync`, `Entity`, `.Handle_Should_RotateTokens_When_RefreshTokenIsActive`, `AppDbContext`, `RefreshTokenRepository`, `.Handle`, `IAuthFinder`, `AppDbContext`, `Domain.Entities`, `RefreshTokenCreationData`, `.RotateRefreshTokenAsync`, `security-middleware-test.example.cs`, `GetCurrentUserQuery`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **What connects `ErrorCatalog`, `ProblemDetails`, `ApiError` to the rest of the system?**
  _314 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `LogoutCommand` be split into smaller, more focused modules?**
  _Cohesion score 0.061016949152542375 - nodes in this community are weakly interconnected._
- **Should `Application` be split into smaller, more focused modules?**
  _Cohesion score 0.05565638233514821 - nodes in this community are weakly interconnected._
- **Should `CreateItemCommand` be split into smaller, more focused modules?**
  _Cohesion score 0.08362369337979095 - nodes in this community are weakly interconnected._