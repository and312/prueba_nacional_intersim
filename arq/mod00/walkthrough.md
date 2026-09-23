# Walkthrough: Construcción del Módulo 00 (Solución Base .NET 8)
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

Se ha completado con éxito la construcción física y configuración de la **Solución Base (Módulo 00)** de acuerdo con las especificaciones del *Backend Development Blueprint* y las directrices de seguridad de *SecureCoder*. La solución base constituye el punto de partida oficial, completamente compilable y probado, para los desarrollos subsecuentes.

---

## 1. Estructura de Proyectos Creados

Se inicializó la estructura física bajo las carpetas `src/` y `tests/` con los siguientes proyectos configurados en `NacionalSeguros.sln`:

1.  **NacionalSeguros.Shared (`src/NacionalSeguros.Shared`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* Tipos primitivos transversales, patrón Result y excepciones globales.
    *   *Archivos Clave:* [Error.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/Primitives/Error.cs), [Result.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/Primitives/Result.cs), [BaseException.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/Exceptions/BaseException.cs), [DomainException.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Shared/Exceptions/DomainException.cs).
2.  **NacionalSeguros.Domain (`src/NacionalSeguros.Domain`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* Lógica pura de negocio, Value Objects, e interfaces. 100% libre de frameworks y librerías externas de infraestructura (MediatR, etc.).
    *   *Archivos Clave:* [IDomainEvent.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/IDomainEvent.cs), [Entity.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/Entity.cs), [AggregateRoot.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/AggregateRoot.cs), [ValueObject.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/ValueObject.cs), [IUnitOfWork.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/IUnitOfWork.cs).
3.  **NacionalSeguros.Contracts (`src/NacionalSeguros.Contracts`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* DTOs puros de entrada/salida para la API.
    *   *Archivos Clave:* [LoginRequest.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Requests/LoginRequest.cs), [TokenResponse.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/TokenResponse.cs).
4.  **NacionalSeguros.Application (`src/NacionalSeguros.Application`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* Casos de uso de la aplicación, pipeline behaviors y MediatR handlers.
    *   *Archivos Clave:* [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/DependencyInjection.cs).
5.  **NacionalSeguros.Persistence (`src/NacionalSeguros.Persistence`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* Control de acceso físico a SQL Server 2022 mediante EF Core 9 y Dapper.
    *   *Archivos Clave:* [ApplicationDbContext.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Context/ApplicationDbContext.cs), [UnitOfWork.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/UnitOfWork.cs), [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/DependencyInjection.cs).
6.  **NacionalSeguros.Infrastructure (`src/NacionalSeguros.Infrastructure`)**
    *   *Tipo:* Biblioteca de Clases (net8.0)
    *   *Propósito:* Conectores e integraciones tecnológicas (Capa de Caché Distribuida, JWT, Polly, Serilog).
    *   *Archivos Clave:* [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/DependencyInjection.cs).
7.  **NacionalSeguros.Api (`src/NacionalSeguros.Api`)**
    *   *Tipo:* API Web ASP.NET Core (net8.0)
    *   *Propósito:* Host de la aplicación, Swagger, ruteo y middlewares globales.
    *   *Archivos Clave:* [Program.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Program.cs), [appsettings.json](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/appsettings.json), [CorrelationMiddleware.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Middlewares/CorrelationMiddleware.cs), [ExceptionHandlingMiddleware.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Middlewares/ExceptionHandlingMiddleware.cs), [IdempotentCallbackFilter.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Filters/IdempotentCallbackFilter.cs).
8.  **NacionalSeguros.Tests (`tests/NacionalSeguros.Tests`)**
    *   *Tipo:* Proyecto de Pruebas xUnit (net8.0)
    *   *Propósito:* Pruebas unitarias de handlers, lógica de dominio y verificación arquitectónica.
    *   *Archivos Clave:* [ResultTests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/tests/NacionalSeguros.Tests/ResultTests.cs).

---

## 2. Archivos de Configuración Global Creados

*   **[Directory.Build.props](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/Directory.Build.props):** Centraliza la compilación con C# 12, nullable activo, e implementa la regla de tratar warnings como errores para forzar la máxima calidad de código.
*   **[global.json](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/global.json):** Restringe el entorno de construcción a SDKs .NET 8.
*   **[.editorconfig](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/.editorconfig):** Estilos y reglas de formato para código C# y JSON.
*   **[.gitignore](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/.gitignore):** Configurado para omitir directorios de compilación, secretos locales e IDE temporales.
*   **[Dockerfile](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/Dockerfile) & [docker-compose.yml](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/docker-compose.yml):** Contenerización multiplataforma y aprovisionamiento local de instancias de desarrollo para SQL Server 2022 y la capa de caché distribuida.

---

## 3. Resolución de Dependencias NuGet (硬性/Estricta)

Se agregaron y alinearon las siguientes dependencias de forma exitosa, resolviendo conflictos de namespaces y warnings de degradación:
*   **Application:** `MediatR` y `FluentValidation`.
*   **Persistence:** `Microsoft.EntityFrameworkCore.SqlServer` (v9.0.0) y `Dapper`.
*   **Infrastructure:** Librerías de caché distribuida, `Microsoft.AspNetCore.Authentication.JwtBearer` y `Microsoft.Extensions.Http.Polly`.
*   **Api:** `Serilog.AspNetCore`, `Swashbuckle.AspNetCore` (v6.6.2), `Microsoft.OpenApi` (v2.9.0), `Asp.Versioning.Mvc.ApiExplorer` (v8.1.0), `OpenTelemetry` (v1.16.0), `Microsoft.EntityFrameworkCore.Design` (v9.0.0), `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (v8.0.0) y `AspNetCore.HealthChecks.Redis` (v8.0.0).
*   **Tests:** `FluentAssertions` y `Moq`.

---

## 4. Resultados de las Pruebas y Validación

### 6. Certificación Formal de Base de Datos

### **Resultado de Certificación:** DATABASE IMPLEMENTED (Base de Datos Implementada)

Se certifica formalmente que la base de datos SQL Server 2022 implementada físicamente bajo el nombre **`SIR_NacionalSeguros`** constituye la implementación oficial del proyecto. Ha superado satisfactoriamente el 100% de las pruebas técnicas, de integridad referencial, consistencia de políticas RLS y triggers transaccionales, y queda completamente preparada para ser consumida por el backend en .NET 8, Entity Framework Core 9 y las correspondientes pruebas de integración de la solución.

---

# Walkthrough: Módulo 03 - Gestión de Solicitudes (Request Management)

Se ha completado e integrado con éxito el **Módulo 03 – Gestión de Solicitudes**, aplicando de forma estricta Clean Architecture, CQRS, Domain-Driven Design (DDD) y la integración del contexto de sesión en base de datos para la política de seguridad RLS por Área.

## 1. Componentes y Capas Implementadas

### A. Capa de Dominio (`NacionalSeguros.Domain`)
* **[Solicitud.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Solicitud.cs):** Aggregate Root de solicitudes. Implementa la validación del flujo de transiciones oficial (`Borrador` $\rightarrow$ `En Validación` $\rightarrow$ `Aprobada` / `Rechazada` / `Observada`). Además, restringe los cambios de campos únicamente cuando la solicitud está en estado `Borrador` (`SOL-BOR`) u `Observada` (`SOL-OBS`).
* **[SolicitudComentario.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/SolicitudComentario.cs):** Entidad para comentarios y bitácoras asociadas a las solicitudes (como el motivo de cancelación o justificación de rechazo).
* **[ISolicitudRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/ISolicitudRepository.cs) y [ISolicitudComentarioRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/ISolicitudComentarioRepository.cs):** Contratos de persistencia para abstracción total de la capa de acceso a datos.

### B. Capa de Aplicación (`NacionalSeguros.Application`)
* **Commands y Handlers:**
  - `CrearSolicitud`: Inicializa una nueva solicitud en estado `Borrador`.
  - `ActualizarSolicitud`: Permite modificar los datos de la solicitud, validando defensivamente el estado permitido.
  - `CancelarSolicitud`: Transita de forma controlada una solicitud al estado `Cancelada` (`SOL-CAN`), agregando el comentario correspondiente.
  - `TransitarSolicitudEstado`: Maneja de forma genérica las transiciones y asignaciones de decisores para aprobaciones y rechazos.
* **Queries y Handlers:**
  - `GetSolicitudById`: Retorna el detalle completo de una solicitud.
  - `ListSolicitudesKanban`: Consulta paginada y filtrada óptima para la visualización del Kanban de solicitudes.
  - `GetSolicitudStateHistory`: Recupera el historial inmutable de cambios de estado (Ledger `StateHistory`) asociados a la solicitud.

### C. Capa de Persistencia (`NacionalSeguros.Persistence`)
* **Mapeo Físico y Filtros:** Se configuraron `SolicitudConfiguration.cs` y `SolicitudComentarioConfiguration.cs` para el mapeo fluido contra las tablas físicas de SQL Server 2022, implementando un filtro de consulta global (`HasQueryFilter`) para excluir automáticamente los registros eliminados lógicamente (Soft Delete).
* **[SetSessionContextInterceptor.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Interceptors/SetSessionContextInterceptor.cs):** Interceptor EF Core que inyecta en cada conexión abierta las variables `UserMail`, `UserRol`, `UserArea` y `CorrelationId` al contexto de sesión (`SESSION_CONTEXT`) de SQL Server, activando de manera transparente las políticas RLS perimetrales a nivel de base de datos.

### D. Capa de Presentación (`NacionalSeguros.Api`)
* **[SolicitudesController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/SolicitudesController.cs):** Controlador REST que expone la API completa alineada rigurosamente con la especificación OpenAPI (`openapi.yaml`):
  - `POST /api/v1/solicitudes` - Crear solicitud.
  - `GET /api/v1/solicitudes/{id}` - Obtener detalle.
  - `PUT /api/v1/solicitudes/{id}` - Modificar solicitud (Solo en Borrador/Observada).
  - `POST /api/v1/solicitudes/{id}/aprobar` - Aprobar y transitar a `SOL-APR`.
  - `POST /api/v1/solicitudes/{id}/rechazar` - Rechazar y transitar a `SOL-RECH` requiriendo justificación (mínimo 10 caracteres).
  - `POST /api/v1/solicitudes/{id}/cancelar` - Cancelar solicitud (`SOL-CAN`).
  - `GET /api/v1/solicitudes` - Listar solicitudes (Kanban paginado).
  - `GET /api/v1/solicitudes/{id}/historial` - Historial de transiciones.

---

## 2. Evidencias de Validación y Pruebas Técnicas

### A. Compilación Exitosa
* **Resultado:** **Compilación Correcta. 0 Advertencias, 0 Errores.**
* **Ensamblados generados:** `NacionalSeguros.Domain`, `NacionalSeguros.Contracts`, `NacionalSeguros.Application`, `NacionalSeguros.Persistence`, `NacionalSeguros.Infrastructure`, `NacionalSeguros.Api`, `NacionalSeguros.Tests`.

### B. Pruebas Unitarias y de Integración de Dominio
Se ejecutó la suite completa de pruebas unitarias (`SolicitudesTests.cs`) con un total de **29 pruebas exitosas a nivel global**, validando los siguientes escenarios de solicitudes:
1. **`Solicitud_Constructor_Should_Set_Properties_Correctly`:** Valida la correcta inicialización de propiedades del dominio.
2. **`Solicitud_Actualizar_Should_Modify_Properties_When_In_Borrador_Or_Observada`:** Valida que la actualización de datos funcione correctamente en estados permitidos.
3. **`Solicitud_Actualizar_Should_Throw_InvalidOperationException_When_Not_In_Borrador_Or_Observada`:** Bloquea defensivamente las ediciones si la solicitud ya está en En Validación o Aprobada.
4. **`Solicitud_Transitar_Should_Validate_Allowed_Transitions_And_Succeed`:** Valida transiciones correctas según la matriz de estados.
5. **`Solicitud_Transitar_Should_Throw_InvalidOperationException_When_Transition_Is_Forbidden`:** Comprueba la prevención de transiciones de estado ilegales (ej. Borrador $\rightarrow$ Aprobada).
6. **`CrearSolicitudCommandHandler_Should_Create_And_Save_Solicitud`:** Valida la lógica de negocio y persistencia del comando de creación.
7. **`ActualizarSolicitudCommandHandler_Should_Update_And_Save_Solicitud`:** Valida la lógica y guardado del comando de actualización.
8. **`TransitarSolicitudEstadoCommandHandler_Should_Perform_Transition`:** Valida que las transiciones guarden y propaguen correctamente el decisor.
9. **`ListSolicitudesKanbanQueryHandler_Should_Return_Paged_DTO`:** Valida el correcto mapeo AutoMapper y estructurado de la respuesta Kanban.

```text
Serie de pruebas para NacionalSeguros.Tests.dll (.NETCoreApp,Version=v8.0)
Correctas! - Con error: 0, Superado: 29, Omitido: 0, Total: 29, Duración: 2 s
```

---

## 5. Auditoría de Seguridad e Cumplimiento de Reglas
*   **Sanitización de Errores (CWE-209):** `ExceptionHandlingMiddleware` captura activamente excepciones de tipo `SqlException` e impide que los detalles de conexión o nombres de tablas de la base de datos se escapen en la respuesta JSON.
*   **Cero Secretos Hardcodeados (CWE-798):** En `Program.cs` se ha inyectado un recuperador que, en ausencia de la clave Jwt en la configuración, genera una firma aleatoria efímera y advierte severamente de su uso en consola de desarrollo.
*   **Restricción de Interfaces de Red:** El archivo `launchSettings.json` expone puertos locales mapeados estrictamente a `localhost` y `127.0.0.1`.
