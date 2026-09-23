# KB_22_MasterData_Refactor_2026 — Refactorización y Gobierno de Datos Maestros

Esta Knowledge Base documenta la refactorización arquitectónica y de persistencia realizada en el **Módulo de Gestión de Solicitudes y Catálogos Maestros** del Sistema Inteligente de Reclutamiento (SIR) en Julio de 2026.

---

## 1. Arquitectura Actual del Backend

El sistema sigue los principios de **Clean Architecture**, **SOLID**, **CQRS (Command Query Responsibility Segregation)** y **DDD (Domain-Driven Design)** utilizando:
* **.NET 8**
* **Entity Framework Core 8** (con configuraciones explícitas por entidad)
* **MediatR** para la orquestación de comandos y consultas desacopladas.
* **AutoMapper** para la proyección limpia hacia DTOs de respuesta.
* **FluentValidation** para la validación declarativa de peticiones.
* **SQL Server 2022** como motor relacional primario.

---

## 2. Estructura Real de la Tabla `Solicitudes`

Se realizó una normalización y reestructuración completa de la tabla `Solicitudes` para admitir perfiles estructurados por IA y delegar valores a tablas maestras físicas en lugar de textos planos en lookup.

### Campos Eliminados (Obsoletos)
Las siguientes columnas de texto plano/datos redundantes fueron eliminadas físicamente de la tabla `Solicitudes`:
* `Area`
* `Modalidad`
* `Skills`
* `FechaIdeal`
* `Jornada`
* `RemuneracionOfrecida`
* `TipoSolicitud`
* `Ubicacion`
* `CreatedBy` (delegado al campo correspondiente en auditoría)

### Nuevos Campos del Perfil (Profesiograma)
Se agregaron columnas descriptivas de texto para estructurar los requerimientos generados o validados por IA:
* `ObjetivoCargo` (NVARCHAR(MAX), NULL)
* `FormacionAcademica` (NVARCHAR(MAX), NULL)
* `ExperienciaMinima` (NVARCHAR(MAX), NULL)
* `ExperienciaIndispensable` (NVARCHAR(MAX), NULL)
* `ConocimientosTecnicos` (NVARCHAR(MAX), NULL)
* `HerramientasSistemas` (NVARCHAR(MAX), NULL)
* `CompetenciasClave` (NVARCHAR(MAX), NULL)
* `DisponibilidadRequerida` (NVARCHAR(MAX), NULL)
* `CriteriosExcluyentes` (NVARCHAR(MAX), NULL)
* `CriteriosDeseables` (NVARCHAR(MAX), NULL)

### Relaciones Físicas y Tablas Maestras Nuevas
Se crearon tres tablas físicas independientes para el gobierno y normalización de catálogos organizacionales y operativos:

1. **`Regionales` (Entidad: `Regional`)**
   * Relación: `Solicitudes.RegionalId` (INT, FK -> `Regionales.Id`)
2. **`TiposSolicitud` (Entidad: `TipoSolicitudEntity`)**
   * Relación: `Solicitudes.TipoSolicitudId` (INT, FK -> `TiposSolicitud.Id`)
3. **`ModalidadesTrabajo` (Entidad: `ModalidadTrabajo`)**
   * Relación: `Solicitudes.ModalidadTrabajoId` (INT, FK -> `ModalidadesTrabajo.Id`)

---

## 3. Contratos de Integración de Vacantes (n8n / Automatización)

### Crear Vacante (`POST /api/internal/vacancy-requests`)
Se utiliza para la creación asíncrona de solicitudes desde canales como WhatsApp integrados por n8n.
* **DTO**: `VacancyRequestCreateDto`
* **Campos Obligatorios (Validación Backend)**:
  * `Title` (mapeado a `Cargo`)
  * `RequestedByUsuarioId` (correo del solicitante)
  * `RequestedByUserId` (correo del creador)
  * `RegionalId`
  * `TipoSolicitudId`
  * `WorkModeId`
  * `VacancyCount` (debe ser >= 1)
  * `Reason` (Motivo de la vacante)
  * `ObjetivoCargo`
  * `ExperienciaMinima`
  * `ConocimientosTecnicos`
  * `MainFunctions` (mapeado a `Funciones`)
  * `Priority` (Valores válidos: `"Baja"`, `"Media"`, `"Alta"`)

#### Payload de Ejemplo
```json
{
  "title": "Desarrollador React Junior",
  "requestedByUsuarioId": "admin@nacionalseguros.com.bo",
  "requestedByUserId": "admin@nacionalseguros.com.bo",
  "reason": "Ampliación de equipo frontend local",
  "regionalId": 1,
  "tipoSolicitudId": 1,
  "workModeId": 1,
  "vacancyCount": 2,
  "seniority": "Junior",
  "priority": "Baja",
  "objetivoCargo": "Desarrollar componentes web modernos utilizando React.",
  "experienciaMinima": "1 año de experiencia práctica con JavaScript.",
  "conocimientosTecnicos": "React, JavaScript, CSS3.",
  "mainFunctions": "1. Maquetar vistas. 2. Implementar integraciones REST.",
  "channel": "WhatsApp",
  "workflowOrigen": "n8n_test_manual"
}
```

### Consultar Campos Disponibles (`GET /api/internal/vacancy-requests/fields`)
Retorna los metadatos y opciones dinámicas (Regionales, Tipos de Solicitud, Modalidades de Trabajo y Listas de Prioridad/Seniority) que la automatización o el frontend deben enviar al crear solicitudes.
* **Nota sobre Prioridad**: La opción `"Critica"` fue removida. Solo se admiten `"Baja"`, `"Media"`, y `"Alta"`.

---

## 4. CRUD de Tablas Maestras (`Regionales`, `TiposSolicitud`, `ModalidadesTrabajo`)

Para permitir la administración total de estas tablas maestras desde el Backoffice, se implementaron endpoints de CRUD completos bajo la ruta `api/v1/`.

### Endpoints Disponibles
Reemplazar `{recurso}` por `regionales`, `tipos-solicitud` o `modalidades-trabajo`:

* **`GET /api/v1/{recurso}`**: Obtiene la lista completa de registros no eliminados. Admite el parámetro de consulta `?soloActivos=true` para mostrar únicamente los registros con `Estado = 'Activo'` (ideal para poblar combos).
* **`GET /api/v1/{recurso}/{id}`**: Obtiene el detalle de un registro por su ID.
* **`POST /api/v1/{recurso}`**: Crea un registro. *(Requiere Rol: Administrador)*.
* **`PUT /api/v1/{recurso}/{id}`**: Actualiza Nombre, Descripción, Estado del registro. Si el registro estaba marcado como eliminado y se actualiza a Estado Activo, realiza una reactivación. *(Requiere Rol: Administrador)*.
* **`DELETE /api/v1/{recurso}/{id}`**: Realiza un borrado lógico del registro (`IsDeleted = true`). *(Requiere Rol: Administrador)*.

### Reglas de Negocio en Escritura (POST / PUT / DELETE)
* **Validación de Código**: El campo `Codigo` es obligatorio y debe ser único. Se impide la creación de códigos duplicados.
* **Validación de Nombre**: El campo `Nombre` es obligatorio y no puede ser nulo o vacío.
* **Protección de Relaciones**: No se permite la eliminación lógica (`DELETE`) de un registro si este ya posee asociaciones activas en la tabla `Solicitudes` (retornará error `400 Bad Request`).

---

## 5. Trazabilidad y Seguridad Inalterada
* **Historial de Estados**: La máquina de estados de solicitudes y la tabla `StateHistory` siguen registrando los cambios de estado sin modificaciones.
* **Auditoría Transversal**: Las operaciones de creación, edición e inactivación de datos maestros persisten los datos del autor (`CreatedBy`, `ModifiedBy`, `DeletedBy`) y marcas temporales utilizando `DateTime.UtcNow`.
* **Row-Level Security (RLS)**: El interceptor RLS se actualizó para filtrar mediante `SolicitanteId` en `Solicitudes` y `SolicitudId` en `Vacantes` enlazando con la política física `dbo.SecurityPolicyArea`.

---

## 6. Mantenimiento de Catálogo de Observaciones (`TiposObservacion`)

El catálogo de tipos de observaciones parametrizables (`TipoObservacion`) se administra por separado a través de su propio controlador de endpoints, dado su acoplamiento específico con las observaciones de perfiles en el Sprint de Perfiles.

### Endpoints Disponibles
* **`GET /api/v1/tipos-observacion`**: Recupera todos los tipos de observación sin paginar.
* **`POST /api/v1/tipos-observacion`**: Crea un tipo de observación. *(Requiere Rol: Administrador)*.
* **`PUT /api/v1/tipos-observacion/{id}`**: Modifica el nombre y descripción del registro. *(Requiere Rol: Administrador)*.
* **`PATCH /api/v1/tipos-observacion/{id}/activar`**: Cambia el estado del registro a "Activo". *(Requiere Rol: Administrador)*.
* **`PATCH /api/v1/tipos-observacion/{id}/inactivar`**: Cambia el estado del registro a "Inactivo". *(Requiere Rol: Administrador)*.

### Reglas y Limitaciones del Mantenimiento
* **CRUD Incompleto**: El backend no provee una acción `DELETE` (física ni lógica) para la entidad `TipoObservacion`. Para mitigar esto en el frontend, el servicio `ObservacionesService` bloquea las llamadas locales de eliminación lanzando un error controlado que instruye al usuario a inactivar el elemento usando el interruptor de estado.
* **Gobierno de Datos**: Todas las peticiones al catálogo viajan con los encabezados JWT de la sesión del administrador y registran los metadatos de auditoría del emisor.

