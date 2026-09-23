# Implementation Plan - Module 03: Gestión de Solicitudes (Request/Solicitud Management)

This implementation plan covers the complete development of **Module 03 – Gestión de Solicitudes** for the **Sistema Inteligente de Reclutamiento (SIR)** of Nacional Seguros. The development adheres to Clean Architecture, Domain-Driven Design (DDD), and CQRS patterns, strictly implementing the official state machine and row-level security (RLS) policies.

---

## User Review Required

> [!IMPORTANT]
> - **OpenAPI vs. Physical Schema Fields:** The OpenAPI specification (`SolicitudCreateDto`) lists a subset of fields (`cargoCodigo`, `areaCodigo`, `gerenciaCodigo`, `unidadesCodigo`, `tipoVacanteCodigo`, `remuneracionOfrecida`, and `justificacion`). However, the database schema `Solicitudes` requires additional fields (`Modalidad`, `Seniority`, `Prioridad`, `FechaIdeal`, `Funciones`, `Skills`) as `NOT NULL`. 
>   - **Approach:** We will include the additional required fields in `SolicitudCreateDto` and the corresponding application commands to satisfy database constraints and fulfill functional PRD requirement `RF-01`.
> - **Row Level Security (RLS) Predicate:** The SQL Server database has active RLS predicates for `Solicitudes` (by `Area`) and `Vacantes` (by parent `Area`), relying on database session context values: `SESSION_CONTEXT('UserRol')`, `SESSION_CONTEXT('UserArea')`, `SESSION_CONTEXT('UserMail')`, and `SESSION_CONTEXT('CorrelationId')`.
>   - **Approach:** We will implement an EF Core connection interceptor `SetSessionContextInterceptor` which will dynamically propagate HTTP context claims (email, roles, area header, and correlation ID) into the database session context whenever a connection is opened.
> - **State Machine Transition Matrix:** The state transitions will be validated strictly on the C# domain layer before saving changes, ensuring compliance with the official state matrix. We will look up the parametric states from the `Estados` catalog in the database (e.g. `SOL-BOR`, `SOL-ENV`, `SOL-APR`, `SOL-OBS`, `SOL-RECH`, `SOL-CAN`, `SOL-CONV`).

---

## Open Questions

None. The functional requirements, database schema details, and state machine rules are fully defined in the `PRD`, `DICCIONARIO_DATOS.md`, and `KB_StateMachine.md`.

---

## Proposed Changes

We will create and modify the following files across the Clean Architecture solution layers:

```
[NacionalSeguros.Domain]       <-- Domain Entities, Repositories, and Events
      │
[NacionalSeguros.Contracts]    <-- DTO Request and Response Models
      │
[NacionalSeguros.Application]  <-- CQRS Commands, Queries, Handlers, and Validators
      │
[NacionalSeguros.Persistence]  <-- EF Core DbContext, Configurations, Repositories, Interceptors
      │
[NacionalSeguros.Api]          <-- Controllers, Endpoints, Dependency Injection
      │
[NacionalSeguros.Tests]        <-- Unit, Integration, and Controller Verification
```

---

### 1. [NacionalSeguros.Domain](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain)

We will define the domain entity structures, aggregate rules, state machine transition checks, and repositories.

#### [NEW] [Solicitud.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Solicitud.cs)
- Represents the `Solicitud` aggregate root.
- Properties:
  - `Cargo` (string) - Título del cargo.
  - `Area` (string) - Área de pertenencia.
  - `SolicitanteId` (int) - FK to `Usuario`.
  - `DecisorId` (int?) - FK to `Usuario`, nullable.
  - `Modalidad` (string) - Checked: `Presencial`, `Teletrabajo`, `Hibrido`.
  - `Seniority` (string) - Checked: `Junior`, `SemiSenior`, `Senior`.
  - `Prioridad` (string) - Checked: `Baja`, `Media`, `Alta`, `Critica`.
  - `FechaIdeal` (DateTime) - Date only.
  - `Funciones` (string) - Text representation.
  - `Skills` (string) - Habilidades requeridas.
  - `EstadoId` (int) - Current state.
  - Standard audit properties: `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `DeletedBy`, `DeletedDate`, `IsDeleted`.
- Domain Methods:
  - `Crear(...)`: Factory/Constructor setting `SOL-BOR` state.
  - `Actualizar(...)`: Allowed only in Borrador (`SOL-BOR`) or Observada (`SOL-OBS`) states.
  - `Cancelar(string motivo, string modificadoPor)`: Transition to Cancelada (`SOL-CAN`).
  - `Transitar(int nuevoEstadoId, int? decisorId, string modificadoPor)`: Validate transitions against the matrix:
    - Borrador (`SOL-BOR`) $\rightarrow$ En Validación (`SOL-ENV`)
    - En Validación (`SOL-ENV`) $\rightarrow$ Aprobada (`SOL-APR`), Rechazada (`SOL-RECH`), Observada (`SOL-OBS`)
    - Observada (`SOL-OBS`) $\rightarrow$ En Validación (`SOL-ENV`)
    - Borrador/En Validación $\rightarrow$ Cancelada (`SOL-CAN`)
- Navigations:
  - `Solicitante` (`Usuario`)
  - `Decisor` (`Usuario?`)
  - `Estado` (`Estado`)
  - `Comentarios` (List of `SolicitudComentario`)

#### [NEW] [SolicitudComentario.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/SolicitudComentario.cs)
- Properties: `ComentarioId` (int PK), `SolicitudId` (int), `UsuarioId` (int), `Texto` (string), `Fecha` (DateTime).
- Navigations: `Solicitud`, `Usuario`.

#### [NEW] [ISolicitudRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/ISolicitudRepository.cs)
- Operations:
  - `GetByIdAsync(int id)`
  - `AddAsync(Solicitud solicitud)`
  - `Update(Solicitud solicitud)`
  - `GetPagedAsync(int pageNumber, int pageSize, int? estadoId, string? search)`
  - `GetStateHistoryAsync(int solicitudId)`
  - `GetEstadoByCodigoAsync(string codigo)`

#### [NEW] [ISolicitudComentarioRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/ISolicitudComentarioRepository.cs)
- Operations:
  - `AddAsync(SolicitudComentario comentario)`
  - `GetBySolicitudIdAsync(int solicitudId)`

#### [NEW] [SolicitudCreadaEvent.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Events/SolicitudCreadaEvent.cs)
- Domain event raised upon creation, implementing `IDomainEvent`.

---

### 2. [NacionalSeguros.Contracts](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts)

We will define DTO request and response models.

#### [NEW] [SolicitudCreateDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Solicitudes/SolicitudCreateDto.cs)
- Properties matching both OpenAPI and DDL required parameters:
  - `cargoCodigo` (string), `areaCodigo` (string), `gerenciaCodigo` (string), `unidadesCodigo` (string), `tipoVacanteCodigo` (string), `remuneracionOfrecida` (decimal), `justificacion` (string), `modalidad` (string), `seniority` (string), `prioridad` (string), `fechaIdeal` (DateTime), `funciones` (string), `skills` (string).

#### [NEW] [SolicitudUpdateDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Solicitudes/SolicitudUpdateDto.cs)
- Properties: `cargoCodigo` (string), `areaCodigo` (string), `remuneracionOfrecida` (decimal), `justificacion` (string), `modalidad` (string), `seniority` (string), `prioridad` (string), `fechaIdeal` (DateTime), `funciones` (string), `skills` (string).

#### [NEW] [SolicitudResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Solicitudes/SolicitudResponseDto.cs)
- Properties: `SolicitudId` (int), `CargoCodigo` (string), `AreaCodigo` (string), `RemuneracionOfrecida` (decimal), `EstadoNombre` (string), `CreatedBy` (string), `CreatedDate` (DateTime), `Prioridad` (string), `Modalidad` (string), `Seniority` (string), `FechaIdeal` (DateTime), `Funciones` (string), `Skills` (string), `DecisorId` (int?).

#### [NEW] [SolicitudStateHistoryDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Solicitudes/SolicitudStateHistoryDto.cs)
- Maps `StateHistory` rows for requests: `StateHistoryId` (long), `FromState` (string), `ToState` (string), `Changer` (string), `Fecha` (DateTime), `Comentario` (string).

---

### 3. [NacionalSeguros.Application](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application)

We will implement the CQRS operations.

#### [NEW] [CrearSolicitudCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CrearSolicitud/CrearSolicitudCommand.cs)
- Record containing the creation parameters and user context email: `SolicitudCreateDto Dto, string CreatedBy`.

#### [NEW] [CrearSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CrearSolicitud/CrearSolicitudCommandHandler.cs)
- Validates the cargo, area, and other parameter codes against catalogs.
- Creates the entity in `SOL-BOR` state.
- Saves via repo and unit of work.

#### [NEW] [CrearSolicitudCommandValidator.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CrearSolicitud/CrearSolicitudCommandValidator.cs)
- Rules:
  - `Justificacion` is required, min 10 chars.
  - `RemuneracionOfrecida` > 0.
  - Check constraints for `Modalidad` (Presencial, Teletrabajo, Hibrido), `Seniority` (Junior, SemiSenior, Senior), `Prioridad` (Baja, Media, Alta, Critica).
  - `FechaIdeal` must be a future date.

#### [NEW] [ActualizarSolicitudCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/ActualizarSolicitud/ActualizarSolicitudCommand.cs)
- Properties: `int Id, SolicitudUpdateDto Dto, string ModifiedBy`.

#### [NEW] [ActualizarSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/ActualizarSolicitud/ActualizarSolicitudCommandHandler.cs)
- Fetches the `Solicitud`.
- Checks if the state is Borrador (`SOL-BOR`) or Observada (`SOL-OBS`). Otherwise fails with `INVALID_STATE_TRANSITION`.
- Performs update and persists.

#### [NEW] [CancelarSolicitudCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CancelarSolicitud/CancelarSolicitudCommand.cs)
- Properties: `int Id, string Motivo, string ModifiedBy`.

#### [NEW] [CancelarSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CancelarSolicitud/CancelarSolicitudCommandHandler.cs)
- Validates state transition to `SOL-CAN`.
- Records transition and saves.

#### [NEW] [TransitarSolicitudEstadoCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/TransitarSolicitud/TransitarSolicitudEstadoCommand.cs)
- Properties: `int Id, string NuevoEstadoCodigo, string? Comentario, int? DecisorId, string ModifiedBy`.

#### [NEW] [TransitarSolicitudEstadoCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/TransitarSolicitud/TransitarSolicitudEstadoCommandHandler.cs)
- Evaluates the current state and validates if the target state transition is permitted.
- Records changes and updates.

#### [NEW] [GetSolicitudByIdQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudById/GetSolicitudByIdQuery.cs)
#### [NEW] [GetSolicitudByIdQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudById/GetSolicitudByIdQueryHandler.cs)
- Returns `SolicitudResponseDto`.

#### [NEW] [ListSolicitudesKanbanQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/ListSolicitudesKanban/ListSolicitudesKanbanQuery.cs)
#### [NEW] [ListSolicitudesKanbanQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/ListSolicitudesKanban/ListSolicitudesKanbanQueryHandler.cs)
- Supports paging, searching, and filtering by state.

#### [NEW] [GetSolicitudStateHistoryQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudStateHistory/GetSolicitudStateHistoryQuery.cs)
#### [NEW] [GetSolicitudStateHistoryQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudStateHistory/GetSolicitudStateHistoryQueryHandler.cs)
- Returns a list of state changes from `StateHistory` table for a Solicitud.

#### [NEW] [SolicitudMappingProfile.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Mappings/SolicitudMappingProfile.cs)
- AutoMapper mappings from Domain to Contracts.

---

### 4. [NacionalSeguros.Persistence](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence)

We will configure EF Core entity definitions and implement connection context injection.

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Context/ApplicationDbContext.cs)
- Register `DbSet<Solicitud>` and `DbSet<SolicitudComentario>`.

#### [NEW] [SolicitudConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/SolicitudConfiguration.cs)
- Fluent API configurations mapping columns and check constraints.
- Soft Delete filter: `builder.HasQueryFilter(s => !s.IsDeleted);`

#### [NEW] [SolicitudComentarioConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/SolicitudComentarioConfiguration.cs)
- Maps relations to `Solicitudes` and `Usuarios`.

#### [NEW] [SolicitudRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/SolicitudRepository.cs)
- Repository pattern concrete implementation.

#### [NEW] [SolicitudComentarioRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/SolicitudComentarioRepository.cs)
- Repository for comments.

#### [NEW] [SetSessionContextInterceptor.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Interceptors/SetSessionContextInterceptor.cs)
- Propagates claims into database `SESSION_CONTEXT` upon connection opening.
- Fetches `HttpContext` from `IHttpContextAccessor`.
- Runs SQL commands to set `UserMail`, `UserRol`, `UserArea`, and `CorrelationId`.

#### [MODIFY] [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/DependencyInjection.cs)
- Register repositories `ISolicitudRepository` and `ISolicitudComentarioRepository`.

---

### 5. [NacionalSeguros.Api](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api)

We will expose the REST interface.

#### [NEW] [SolicitudesController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/SolicitudesController.cs)
- Exposes endpoints:
  - `POST /api/v1/solicitudes`: Create.
  - `GET /api/v1/solicitudes/{id}`: Get.
  - `PUT /api/v1/solicitudes/{id}`: Update.
  - `POST /api/v1/solicitudes/{id}/aprobar`: Approve (transits to `SOL-APR`).
  - `POST /api/v1/solicitudes/{id}/rechazar`: Reject (transits to `SOL-RECH`).
  - `POST /api/v1/solicitudes/{id}/cancelar`: Cancel (transits to `SOL-CAN`).
  - `POST /api/v1/solicitudes/{id}/transicion`: Transitar (transits to state parameter).
  - `GET /api/v1/solicitudes`: GetPaged.
  - `GET /api/v1/solicitudes/{id}/historial`: Historial.

#### [MODIFY] [Program.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Program.cs)
- Register `IHttpContextAccessor`.
- Attach `SetSessionContextInterceptor` to DbContext options builder.

---

### 6. [NacionalSeguros.Tests](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/tests/NacionalSeguros.Tests)

We will verify logic and behavior.

#### [NEW] [SolicitudesTests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/tests/NacionalSeguros.Tests/Solicitudes/SolicitudesTests.cs)
- **Unit Tests:**
  - `CrearSolicitud_ShouldSetInitialBorradorState`.
  - `ActualizarSolicitud_ShouldFail_WhenNotInBorradorOrObservada`.
  - `TransitarEstado_ShouldEnforceStateMachineRules` (e.g. Borrador to Aprobada is forbidden).
  - `CrearSolicitudValidator_ShouldEnforceCheckConstraints`.
- **Integration/Repository Tests:**
  - Test context database RLS filtering by Mocking HttpContext headers for different Roles and Areas.

---

## Verification Plan

### Automated Tests
- Build verification: `dotnet build` from root to ensure zero warnings/errors.
- Execute unit and integration tests: `dotnet test` to run all verified test scenarios.

### Manual Verification
- Deploy locally and invoke `POST /api/v1/solicitudes` using Swagger.
- Trigger invalid state transitions (e.g., Borrador -> Aprobada) and verify `400 Bad Request` with `INVALID_STATE_TRANSITION` error code.
- Check database `StateHistory` and `AuditLogs` tables after updates to verify triggers and context variables populate correctly.
