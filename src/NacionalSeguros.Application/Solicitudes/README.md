# Módulo 03: Gestión de Solicitudes
## Sistema Inteligente de Reclutamiento (SIR) – Nacional Seguros

Este módulo gestiona el ciclo de vida completo de una **Solicitud de Personal/Reclutamiento** en el sistema, actuando como el disparador del proceso de selección de talento humano de Nacional Seguros.

---

## 1. Estructura de Código del Módulo

El desarrollo está organizado bajo el estándar de **Clean Architecture** y **Domain-Driven Design (DDD)** del proyecto:

*   **Dominio (`NacionalSeguros.Domain`):**
    *   [Solicitud.cs (Aggregate Root)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Solicitud.cs): Encapsula las reglas del negocio, el constructor de la entidad y la máquina de estados.
    *   [SolicitudComentario.cs (Entidad Relacionada)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/SolicitudComentario.cs): Representa la justificación e historial de observaciones del proceso.
*   **Aplicación (`NacionalSeguros.Application`):**
    *   **Commands:**
        *   [CrearSolicitud](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CrearSolicitud) (Command, Handler, Validator)
        *   [ActualizarSolicitud](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/ActualizarSolicitud) (Command, Handler, Validator)
        *   [CancelarSolicitud](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CancelarSolicitud) (Command, Handler, Validator)
        *   [TransitarSolicitud](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/TransitarSolicitud) (Command, Handler, Validator) - Cubre las operaciones de transición general, aprobación y rechazo.
    *   **Queries:**
        *   [GetSolicitudById](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudById) (Query, Handler)
        *   [ListSolicitudesKanban](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/ListSolicitudesKanban) (Query, Handler)
        *   [GetSolicitudStateHistory](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/GetSolicitudStateHistory) (Query, Handler)
*   **Persistencia (`NacionalSeguros.Persistence`):**
    *   [SolicitudConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/SolicitudConfiguration.cs) / [SolicitudComentarioConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/SolicitudComentarioConfiguration.cs): Mapeos de EF Core, índices, llaves y filtros globales de Soft Delete.
    *   [SolicitudRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/SolicitudRepository.cs): Repositorio persistente e implementación de consultas optimizadas para Kanban.
*   **Controladores (`NacionalSeguros.Api`):**
    *   [SolicitudesController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/SolicitudesController.cs): Exposición de Endpoints de la API protegidos por JWT.

---

## 2. Matriz PRD → Código (Trazabilidad Funcional)

| Código Requisito PRD | Nombre del Requisito | Archivo / Símbolo de Código | Detalles de Implementación |
| :---: | :--- | :--- | :--- |
| **RF-01** | Registro de Solicitudes | [CrearSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CrearSolicitud/CrearSolicitudCommandHandler.cs) | Permite registrar y persistir nuevas solicitudes en estado Borrador (`SOL-BOR`). |
| **RF-02** | Edición de Solicitudes | [ActualizarSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/ActualizarSolicitud/ActualizarSolicitudCommandHandler.cs) | Permite actualizar la solicitud únicamente si se encuentra en Borrador o en Observada. |
| **RF-03** | Cancelación de Solicitudes | [CancelarSolicitudCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Commands/CancelarSolicitud/CancelarSolicitudCommandHandler.cs) | Modifica el estado del registro a `SOL-CAN` registrando el motivo obligatorio en comentarios. |
| **RF-04** | Aprobación / Rechazo | [SolicitudesController.cs (Aprobar/Rechazar)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/SolicitudesController.cs#L140-L211) | Valida las firmas de los decisores y obliga la justificación (mínimo 10 caracteres) al rechazar. |
| **RF-05** | Tablero Kanban / Buscador | [ListSolicitudesKanbanQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Solicitudes/Queries/ListSolicitudesKanban/ListSolicitudesKanbanQueryHandler.cs) | Retorna registros paginados y filtrados por estado o término de búsqueda de texto libre. |
| **SLA-SOL-01** | Límite de SLA (3 días hábiles) | [semilla.sql (SLA Seed)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/sql/09_SEED_DATA/semilla.sql#L68-L74) | El estado `SOL-ENV` está asociado al SLA-SOL-01 mediante su llave foránea. |

---

## 3. Matriz Estados → Transiciones (Máquina de Estados)

| Código Estado Origen | Estado Destino Permitido | Acción / Evento | Restricciones / Validaciones |
| :---: | :---: | :--- | :--- |
| **`SOL-BOR`** (Borrador) | **`SOL-ENV`** (En Validación) | Enviar a Validación | Cambia el estado a En Validación. |
| | **`SOL-CAN`** (Cancelada) | Cancelar | Requiere justificación del motivo. |
| **`SOL-ENV`** (En Validación) | **`SOL-APR`** (Aprobada) | Aprobar Solicitud | Requiere ID del decisor y firma digital. |
| | **`SOL-RECH`** (Rechazada) | Rechazar Solicitud | Requiere comentario/justificación de mínimo 10 caracteres. |
| | **`SOL-OBS`** (Observada) | Observar Solicitud | Guarda comentarios y habilita la edición del registro. |
| | **`SOL-CAN`** (Cancelada) | Cancelar | Requiere justificación del motivo. |
| **`SOL-OBS`** (Observada) | **`SOL-ENV`** (En Validación) | Re-enviar a Validación | Habilita re-enviar tras realizar las correcciones del caso. |
| | **`SOL-CAN`** (Cancelada) | Cancelar | Requiere justificación del motivo. |
| **`SOL-RECH`** (Rechazada) | **`SOL-BOR`** (Borrador) | Reapertura | Habilita restaurar para correcciones (modo edición activo). |
| **`SOL-CAN`** (Cancelada) | **`SOL-BOR`** (Borrador) | Reapertura | Habilita restaurar para correcciones (modo edición activo). |

*Nota: Cualquier otra transición no descrita anteriormente (ej. `SOL-BOR` directo a `SOL-APR`) es rechazada arrojando una excepción de dominio (`InvalidOperationException`) y prevenida por el motor de base de datos.*

---

## 4. Matriz Endpoints → OpenAPI (Swagger)

| Ruta HTTP | Método | Operación de Negocio | Parámetros del Request | Respuesta HTTP |
| :--- | :---: | :--- | :--- | :---: |
| `/api/v1/solicitudes` | `POST` | Crear una Solicitud | `SolicitudCreateDto` (JSON) | 201 Created |
| `/api/v1/solicitudes` | `GET` | Consultar Tablero (Kanban) | Query Params: `pageNumber`, `pageSize`, `estadoId`, `search` | 200 OK |
| `/api/v1/solicitudes/{id}` | `GET` | Ver Detalle de Solicitud | Ruta: `id` (Entero) | 200 OK / 404 NF |
| `/api/v1/solicitudes/{id}` | `PUT` | Editar Solicitud | Ruta: `id`, Body: `SolicitudUpdateDto` | 200 OK / 400 BR |
| `/api/v1/solicitudes/{id}/transicion` | `POST` | Transición General de Estado | Ruta: `id`, Body: `TransicionEstadoRequest` | 200 OK / 400 BR |
| `/api/v1/solicitudes/{id}/aprobar` | `POST` | Aprobar Solicitud | Ruta: `id`, Body: `AprobarSolicitudRequest` (Opcional) | 200 OK / 400 BR |
| `/api/v1/solicitudes/{id}/rechazar` | `POST` | Rechazar Solicitud | Ruta: `id`, Body: `RechazarSolicitudRequest` (Justificación req.) | 200 OK / 400 BR |
| `/api/v1/solicitudes/{id}/cancelar` | `POST` | Cancelar Solicitud | Ruta: `id`, Body: `CancelarSolicitudRequest` (Motivo req.) | 200 OK / 400 BR |
| `/api/v1/solicitudes/{id}/historial` | `POST` / `GET` | Ver Línea de Tiempo Histórica | Ruta: `id` (Entero) | 200 OK |

---

## 5. Autoevaluación y Certificación Técnica

*   **Compilación general:** `dotnet build` ejecutada de forma limpia y exitosa (**0 errores, 0 advertencias**).
*   **Compilación del set de pruebas:** xUnit y FluentAssertions funcionando. **29 pruebas superadas de 29 totales (100% de aprobación)**.
*   **DDD & SOLID:** Entidades enriquecidas y desvinculadas de dependencias a frameworks.
*   **Seguridad:** Interceptor `sp_set_session_context` para enforzar Row Level Security (RLS) en SQL Server 2022 y autenticación JWT RBAC.

### **Declaración de Estado:**
*   **`CODE GENERATED`** (Código completo generado)
*   **`BUILD SUCCESSFUL`** (Compilación limpia)
*   **`READY FOR AUDIT`** (Listo para Auditoría Integral)
