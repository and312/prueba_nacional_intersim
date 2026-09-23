# Plan de Implementación: Módulo 00 - Solución Base .NET 8
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

Este plan detalla el orden de creación, estructuración, dependencias y configuración de la solución base del **Sistema Inteligente de Reclutamiento (SIR)** utilizando **.NET 8**, **C# 12** y **Entity Framework Core 9**.

---

## User Review Required

> [!IMPORTANT]
> - **Entorno de Compilación:** Se asume que el SDK de .NET 8 (o superior compatible) está instalado en el sistema de desarrollo.
> - **Base de Datos y Caché Distribuida:** Se asume que SQL Server 2022 y la capa de caché distribuida están disponibles (ya sea localmente o mediante la configuración en el `docker-compose.yml` provisto). En local, EF Core no ejecutará automáticamente las migraciones contra SQL Server, sino que se proporcionará el contexto listo para que puedan aplicarse.
> - **Criptografía (Always Encrypted):** La configuración de Always Encrypted se preparará para permitir una desconexión elegante (graceful fallback) en desarrollo local cuando no se disponga de los certificados del proveedor corporativo de gestión de secretos.

---

## Open Questions

- Ninguno por el momento. La configuración de infraestructura base seguirá estrictamente el `Backend Development Blueprint` aprobado.

---

## Proposed Changes

La solución física se organizará en el directorio del workspace, separando el código fuente bajo `src/` y las pruebas bajo `tests/`.

### 1. Inicialización y Archivos de Configuración de la Solución
Se inicializará la solución `NacionalSeguros.sln` y los 8 proyectos especificados.

* **[NEW] [Directory.Build.props](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/Directory.Build.props):** Archivo para centralizar la versión de C# 12, .NET 8, tratamiento de warnings como errores, nullable y habilitar el control de compilación global.
* **[NEW] [global.json](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/global.json):** Bloquear el SDK de .NET a la versión 8.0.
* **[NEW] [.editorconfig](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/.editorconfig):** Estándares de codificación y formateo en C# (PascalCase, llaves, etc.).
* **[NEW] [.gitignore](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/.gitignore):** Exclusiones estándar de .NET, archivos binarios de compilación (`bin`, `obj`), variables de secretos de usuario y configuraciones locales.
* **[NEW] [Dockerfile](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/Dockerfile):** Archivo de Docker multipaso optimizado para compilar y ejecutar el API de producción.
* **[NEW] [docker-compose.yml](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/docker-compose.yml):** Orquestación local para levantar servicios de base de datos SQL Server 2022 y la capa de caché distribuida para caché y tokens.

---

### 2. Proyectos y Dependencias (`src/` y `tests/`)
Se crearán los 8 proyectos con las siguientes referencias:

#### Capa Shared
* **[NEW] [NacionalSeguros.Shared](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/NacionalSeguros.Shared.csproj):**
  - Sin dependencias de proyectos.
  - Implementará el patrón `Result.cs`, excepciones genéricas `BaseException.cs` y tipos comunes.

#### Capa Domain
* **[NEW] [NacionalSeguros.Domain](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/NacionalSeguros.Domain.csproj):**
  - Dependencia: `NacionalSeguros.Shared`.
  - Implementará primitivas DDD (`Entity.cs`, `AggregateRoot.cs`, `ValueObject.cs`, `DomainEvent.cs`).
  - No depende de frameworks de infraestructura.

#### Capa Contracts
* **[NEW] [NacionalSeguros.Contracts](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/NacionalSeguros.Contracts.csproj):**
  - Dependencia: `NacionalSeguros.Shared`.
  - Definirá DTOs puros de Requests y Responses para el API.

#### Capa Application
* **[NEW] [NacionalSeguros.Application](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/NacionalSeguros.Application.csproj):**
  - Dependencias: `NacionalSeguros.Domain`, `NacionalSeguros.Contracts`, `NacionalSeguros.Shared`.
  - Configura MediatR, FluentValidation, AutoMapper.
  - Implementa los Pipeline Behaviors (`LoggingBehavior`, `ValidationBehavior`).

#### Capa Persistence
* **[NEW] [NacionalSeguros.Persistence](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/NacionalSeguros.Persistence.csproj):**
  - Dependencias: `NacionalSeguros.Domain`, `NacionalSeguros.Application`, `NacionalSeguros.Shared`.
  - Implementa `ApplicationDbContext.cs` heredando de EF Core 9 y Dapper.
  - Setup base de `UnitOfWork` y repositorios genéricos.

#### Capa Infrastructure
* **[NEW] [NacionalSeguros.Infrastructure](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/NacionalSeguros.Infrastructure.csproj):**
  - Dependencias: `NacionalSeguros.Application`, `NacionalSeguros.Domain`, `NacionalSeguros.Shared`.
  - Implementa JWT Token, Polly Client wrapper para n8n, la capa de caché distribuida wrapper y configuración de Serilog.

#### Capa API
* **[NEW] [NacionalSeguros.Api](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/NacionalSeguros.Api.csproj):**
  - Dependencias: `NacionalSeguros.Application`, `NacionalSeguros.Persistence`, `NacionalSeguros.Infrastructure`, `NacionalSeguros.Contracts`.
  - Implementa el host, middleware de excepciones, logs de correlation, versionamiento e inyección de dependencias global en `Program.cs`.

#### Capa de Pruebas
* **[NEW] [NacionalSeguros.Tests](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/tests/NacionalSeguros.Tests/NacionalSeguros.Tests.csproj):**
  - Dependencia: `NacionalSeguros.Api`.
  - Contiene xUnit, FluentAssertions y Moq para pruebas de arquitectura y unitarias de infraestructura.

---

### 3. Implementación de Código de Infraestructura y Base (Módulo 00)

* **[NEW] [src/NacionalSeguros.Shared/Primitives/Result.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/Primitives/Result.cs):** Primitiva para el patrón Result que previene el lanzamiento de excepciones para flujos lógicos controlados.
* **[NEW] [src/NacionalSeguros.Domain/Primitives/Entity.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/Entity.cs):** Clase base de entidad con ID e historial de DomainEvents.
* **[NEW] [src/NacionalSeguros.Persistence/Context/ApplicationDbContext.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Context/ApplicationDbContext.cs):** Setup inicial de EF Core 9.
* **[NEW] [src/NacionalSeguros.Persistence/Repositories/UnitOfWork.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/UnitOfWork.cs):** Coordinador de transacciones de base de datos.
* **[NEW] [src/NacionalSeguros.Api/Middlewares/ExceptionHandlingMiddleware.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Middlewares/ExceptionHandlingMiddleware.cs):** Middleware de captura y sanitización de errores.
* **[NEW] [src/NacionalSeguros.Api/Middlewares/CorrelationMiddleware.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Middlewares/CorrelationMiddleware.cs):** Gestión de CorrelationId para trazabilidad.
* **[NEW] [src/NacionalSeguros.Api/Program.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Program.cs):** Configuración de contenedores DI, Serilog, OpenTelemetry, autenticación JWT, Swagger y Health Checks.

---

## Verification Plan

### Automated Tests
Se ejecutarán los siguientes comandos en PowerShell desde el directorio de trabajo principal:
1. `dotnet build` para asegurar la compilación 100% libre de advertencias y errores.
2. `dotnet test` para asegurar que el setup de pruebas reconozca el runner.

### Manual Verification
- Comprobar que la solución se compile correctamente y que las referencias Clean Architecture no tengan dependencias circulares.
- Validar mediante el log que Serilog y el DbContext estén registrados en la DI de `Program.cs`.

### Security Section (Mandatory Rules)
- **Zero Secrets in Code:** Validar que los valores de tokens JWT, llaves LDAP y credenciales de base de datos se consuman exclusivamente de `Configuration` (con origen en variables de entorno o Secrets Manager), sin fallbacks literales inseguros en duro.
- **SQL Sanitization:** Confirmar que no se usará SQL Dinámico concatenado. El DbContext y Dapper parametrizarán cualquier consulta física.
- **Exception Sanitization:** El middleware de excepciones interceptará los errores, capturando las excepciones de base de datos (`SqlException`) y mapeándolas a una respuesta genérica sanitizada de error para impedir fugas de metadatos (nombres de tablas, columnas, etc.).
- **Serilog Masking:** El formateador de Serilog será configurado para enmascarar u omitir cualquier header de autorización o propiedad de login/tokens JWT en caso de logging de solicitudes.
- **Localhost Bound:** Durante pruebas locales, configurar `launchSettings.json` de la API para enlazarse exclusivamente a `127.0.0.1` o `localhost` y nunca a `0.0.0.0`.
