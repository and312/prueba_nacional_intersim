# Diseño Físico de Base de Datos SQL Server 2022
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

Este documento define la especificación técnica formal y estructurada del **Diseño Físico de Base de Datos** del **Sistema Inteligente de Reclutamiento (SIR)** para **Nacional Seguros**. El diseño se basa en **Microsoft SQL Server 2022 Standard** con un nivel de compatibilidad **160** y está mapeado para integrarse con **Entity Framework Core 9** y **.NET 8**.

---

## 1. Estándares de Nomenclatura

Para garantizar la consistencia, el mantenimiento y la legibilidad del modelo de datos, se aplican los siguientes estándares de nomenclatura obligatorios:

* **Tablas:** Nombres en plural, en formato `PascalCase` (ej. `Usuarios`, `Solicitudes`, `Feriados`).
* **Columnas:** Nombres en singular, en formato `PascalCase` (ej. `UsuarioId`, `CreatedDate`, `IsActive`).
* **Claves Primarias (PK):** Nombradas con el prefijo `PK_` seguido del nombre de la tabla (ej. `PK_Usuarios`).
* **Claves Foráneas (FK):** Nombradas con el prefijo `FK_` seguido del nombre de la tabla de origen y la de destino (ej. `FK_Solicitudes_Usuarios`).
* **Constraints Unique:** Nombradas con el prefijo `UQ_` seguido del nombre de la tabla y la columna (ej. `UQ_Usuarios_Correo`).
* **Constraints Check:** Nombradas con el prefijo `CK_` seguido del nombre de la tabla y la lógica evaluada (ej. `CK_Usuarios_TipoAutenticacion`).
* **Índices Clustered:** Prefijo `PK_` (coincidente con la llave primaria) o `CIX_` en caso de índices agrupados personalizados.
* **Índices Non-Clustered:** Prefijo `IX_` seguido de la tabla y las columnas indexadas (ej. `IX_Usuarios_Correo`).
* **Índices Filtrados:** Prefijo `FIX_` seguido de la tabla y columnas con la condición (ej. `FIX_Usuarios_ActiveDirectoryId_NotNull`).
* **Vistas:** Prefijo `vw_` seguido de una descripción descriptiva en `PascalCase` (ej. `vw_PostulantePipeline`).
* **Procedimientos Almacenados (Stored Procedures):** Prefijo `sp_` seguido de la acción y el contexto en `PascalCase` (ej. `sp_Seguridad_ObtenerPermisosUsuario`).

---

## 2. Catálogo de Tablas del Modelo Físico

A continuación se presenta la especificación detallada de cada una de las tablas del sistema agrupadas por su dominio funcional. Todas las llaves primarias de tipo entero autoincrementales utilizan la cláusula `IDENTITY(1,1)`.

### 2.1 Dominio: Seguridad y Accesos

#### Tabla: `Usuarios`
* **Nombre Físico:** `Usuarios`
* **Columnas:**
  - `UsuarioId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Usuarios`) | Identificador único incremental.
  - `Nombre` | `NVARCHAR(150)` | `NOT NULL` | Nombre completo del usuario.
  - `Correo` | `NVARCHAR(100)` | `NOT NULL` | Correo electrónico institucional.
  - `ClaveHash` | `NVARCHAR(256)` | `NULL` | Contraseña cifrada (local). Obligatoria si `TipoAutenticacion = 'Local'`.
  - `TipoAutenticacion` | `NVARCHAR(30)` | `NOT NULL` | Default: `'Local'`. Valores: `'Local'`, `'ActiveDirectory'`.
  - `ActiveDirectoryId` | `NVARCHAR(100)` | `NULL` | Identificador del SID o GUID en el AD de Nacional Seguros.
  - `Estado` | `NVARCHAR(20)` | `NOT NULL` | Default: `'Activo'`. Valores: `'Activo'`, `'Inactivo'`.
  - `MfaHabilitado` | `BIT` | `NOT NULL` | Default: `0`. Bandera de doble factor activa.
  - `MfaSecreto` | `NVARCHAR(128)` | `NULL` | Clave secreta cifrada para la OTP de Google Authenticator.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL` | Usuario que creó el registro.
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`. Fecha de creación.
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL` | Usuario que modificó el registro.
  - `ModifiedDate` | `DATETIME2(7)` | `NULL` | Fecha de modificación.
  - `DeletedBy` | `NVARCHAR(100)` | `NULL` | Usuario que borró lógicamente el registro.
  - `DeletedDate` | `DATETIME2(7)` | `NULL` | Fecha del borrado lógico.
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`. Bandera de borrado lógico.
* **Constraints:**
  - `UQ_Usuarios_Correo`: Unique Constraint sobre `Correo` para evitar duplicados.
  - `CK_Usuarios_ClaveHash_AD`: Check Constraint: `((TipoAutenticacion = 'Local' AND ClaveHash IS NOT NULL) OR (TipoAutenticacion = 'ActiveDirectory' AND ClaveHash IS NULL))`.
  - `CK_Usuarios_Estado`: Check Constraint: `(Estado IN ('Activo', 'Inactivo'))`.
  - `CK_Usuarios_TipoAutenticacion`: Check Constraint: `(TipoAutenticacion IN ('Local', 'ActiveDirectory'))`.

#### Tabla: `Roles`
* **Nombre Físico:** `Roles`
* **Columnas:**
  - `RolId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Roles`) | Identificador único.
  - `Nombre` | `NVARCHAR(50)` | `NOT NULL` | Nombre del rol (ej: `'Administrador'`, `'RRHH'`, `'Reclutador'`, `'Decisor'`, `'Auditor'`).
  - `Descripcion` | `NVARCHAR(250)` | `NULL` | Explicación de los alcances del rol.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Roles_Nombre`: Unique Constraint sobre `Nombre`.

#### Tabla: `UsuarioRoles`
* **Nombre Físico:** `UsuarioRoles`
* **Columnas:**
  - `UsuarioId` | `INT` | `NOT NULL` | Llave Primaria y Foránea (`FK_UsuarioRoles_Usuarios`).
  - `RolId` | `INT` | `NOT NULL` | Llave Primaria y Foránea (`FK_UsuarioRoles_Roles`).
* **Constraints:**
  - `PK_UsuarioRoles`: Primary Key compuesta por `(UsuarioId, RolId)`.

#### Tabla: `Permisos`
* **Nombre Físico:** `Permisos`
* **Columnas:**
  - `PermisoId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Permisos`).
  - `Codigo` | `NVARCHAR(100)` | `NOT NULL` | Código de control del claim (ej. `'solicitudes.crear'`).
  - `Nombre` | `NVARCHAR(100)` | `NOT NULL` | Nombre descriptivo del permiso.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Permisos_Codigo`: Unique Constraint sobre `Codigo`.

#### Tabla: `RolPermisos`
* **Nombre Físico:** `RolPermisos`
* **Columnas:**
  - `RolId` | `INT` | `NOT NULL` | Llave Primaria y Foránea (`FK_RolPermisos_Roles`).
  - `PermisoId` | `INT` | `NOT NULL` | Llave Primaria y Foránea (`FK_RolPermisos_Permisos`).
* **Constraints:**
  - `PK_RolPermisos`: Primary Key compuesta por `(RolId, PermisoId)`.

#### Tabla: `Sesiones`
* **Nombre Físico:** `Sesiones`
* **Columnas:**
  - `SesionId` | `BIGINT` | `NOT NULL` | Llave Primaria (`PK_Sesiones`).
  - `UsuarioId` | `INT` | `NOT NULL` | Foránea (`FK_Sesiones_Usuarios`).
  - `RefreshToken` | `NVARCHAR(256)` | `NOT NULL` | Token criptográfico de renovación.
  - `FechaExpiracion` | `DATETIME2(7)` | `NOT NULL` | Fecha límite del refresh token.
  - `Activa` | `BIT` | `NOT NULL` | Default: `1`. Indica si el token es válido o fue consumido/anulado.
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
* **Constraints:**
  - `UQ_Sesiones_RefreshToken`: Unique Constraint sobre `RefreshToken`.

---

### 2.2 Dominio: Solicitudes de Personal

#### Tabla: `Solicitudes`
* **Nombre Físico:** `Solicitudes`
* **Columnas:**
  - `SolicitudId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Solicitudes`).
  - `Cargo` | `NVARCHAR(100)` | `NOT NULL` | Nombre comercial del cargo.
  - `Area` | `NVARCHAR(100)` | `NOT NULL` | Departamento al que pertenece la vacante.
  - `SolicitanteId` | `INT` | `NOT NULL` | Foránea (`FK_Solicitudes_Usuarios_Solicitante`) que apunta al usuario creador.
  - `DecisorId` | `INT` | `NULL` | Foránea (`FK_Solicitudes_Usuarios_Decisor`) que apunta al aprobador.
  - `Modalidad` | `NVARCHAR(50)` | `NOT NULL` | Valores: `'Presencial'`, `'Teletrabajo'`, `'Hibrido'`.
  - `Seniority` | `NVARCHAR(50)` | `NOT NULL` | Valores: `'Junior'`, `'SemiSenior'`, `'Senior'`.
  - `Prioridad` | `NVARCHAR(20)` | `NOT NULL` | Valores: `'Baja'`, `'Media'`, `'Alta'`, `'Critica'`.
  - `FechaIdeal` | `DATE` | `NOT NULL` | Fecha estimada en que se requiere cubrir el cargo.
  - `Funciones` | `NVARCHAR(MAX)` | `NOT NULL` | Descripción textual de las responsabilidades.
  - `Skills` | `NVARCHAR(MAX)` | `NOT NULL` | Habilidades requeridas especificadas en la solicitud.
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_Solicitudes_Estados`) al catálogo de estados.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `DeletedBy` | `NVARCHAR(100)` | `NULL`
  - `DeletedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `CK_Solicitudes_Modalidad`: Check Constraint: `(Modalidad IN ('Presencial', 'Teletrabajo', 'Hibrido'))`.
  - `CK_Solicitudes_Seniority`: Check Constraint: `(Seniority IN ('Junior', 'SemiSenior', 'Senior'))`.
  - `CK_Solicitudes_Prioridad`: Check Constraint: `(Prioridad IN ('Baja', 'Media', 'Alta', 'Critica'))`.

#### Tabla: `SolicitudComentarios`
* **Nombre Físico:** `SolicitudComentarios`
* **Columnas:**
  - `ComentarioId` | `INT` | `NOT NULL` | Llave Primaria (`PK_SolicitudComentarios`).
  - `SolicitudId` | `INT` | `NOT NULL` | Foránea (`FK_SolicitudComentarios_Solicitudes`).
  - `UsuarioId` | `INT` | `NOT NULL` | Foránea (`FK_SolicitudComentarios_Usuarios`).
  - `Texto` | `NVARCHAR(1000)` | `NOT NULL` | Contenido del comentario.
  - `Fecha` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.

---

### 2.3 Dominio: Perfiles de Cargo (Profesiogramas)

#### Tabla: `PerfilesCargo`
* **Nombre Físico:** `PerfilesCargo`
* **Columnas:**
  - `PerfilCargoId` | `INT` | `NOT NULL` | Llave Primaria (`PK_PerfilesCargo`).
  - `SolicitudId` | `INT` | `NOT NULL` | Foránea (`FK_PerfilesCargo_Solicitudes`).
  - `Cargo` | `NVARCHAR(100)` | `NOT NULL`
  - `Descripcion` | `NVARCHAR(MAX)` | `NOT NULL` | Profesiograma estructurado generado por la IA y editado por RRHH.
  - `Version` | `INT` | `NOT NULL` | Default: `1`. Número correlativo de versión.
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_PerfilesCargo_Estados`).
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_PerfilesCargo_Solicitud_Version`: Unique Constraint en `(SolicitudId, Version)`.

---

### 2.4 Dominio: Gestión de Vacantes

#### Tabla: `Vacantes`
* **Nombre Físico:** `Vacantes`
* **Columnas:**
  - `VacanteId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Vacantes`).
  - `PerfilCargoId` | `INT` | `NOT NULL` | Foránea (`FK_Vacantes_PerfilesCargo`).
  - `SolicitudId` | `INT` | `NOT NULL` | Foránea (`FK_Vacantes_Solicitudes`).
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_Vacantes_Estados`).
  - `FechaApertura` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
  - `FechaCierre` | `DATETIME2(7)` | `NULL` | Fecha de cierre definitivo de la vacante.
  - `BandaSalarialMin` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** | Salario mínimo asignado al puesto.
  - `BandaSalarialMax` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** | Salario máximo asignado al puesto.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`

---

### 2.5 Dominio: Gestión de Postulantes

#### Tabla: `Postulantes`
* **Nombre Físico:** `Postulantes`
* **Columnas:**
  - `PostulanteId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Postulantes`).
  - `Nombres` | `NVARCHAR(100)` | `NOT NULL`
  - `Apellidos` | `NVARCHAR(100)` | `NOT NULL`
  - `Correo` | `NVARCHAR(100)` | `NOT NULL`
  - `DocumentoIdentidad` | `NVARCHAR(30)` | `NOT NULL` | Cédula de Identidad o pasaporte.
  - `Origen` | `NVARCHAR(50)` | `NOT NULL` | Default: `'LinkedIn'`. Valores: `'LinkedIn'`, `'Web'`, `'Referido'`, `'Manual'`.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Postulantes_Documento`: Unique Constraint sobre `DocumentoIdentidad`.
  - `CK_Postulantes_Origen`: Check Constraint: `(Origen IN ('LinkedIn', 'Web', 'Referido', 'Manual'))`.

#### Tabla: `Postulaciones`
* **Nombre Físico:** `Postulaciones`
* **Columnas:**
  - `PostulacionId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Postulaciones`).
  - `PostulanteId` | `INT` | `NOT NULL` | Foránea (`FK_Postulaciones_Postulantes`).
  - `VacanteId` | `INT` | `NOT NULL` | Foránea (`FK_Postulaciones_Vacantes`).
  - `FechaPostulacion` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
  - `EstadoPipelineId` | `INT` | `NOT NULL` | Foránea (`FK_Postulaciones_Estados`).
  - `PretensionSalarial` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** (Always Encrypted).
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Postulaciones_Postulante_Vacante`: Unique Constraint en `(PostulanteId, VacanteId)`.

#### Tabla: `Matchings`
* **Nombre Físico:** `Matchings`
* **Columnas:**
  - `MatchingId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Matchings`).
  - `PostulanteId` | `INT` | `NOT NULL` | Foránea (`FK_Matchings_Postulantes`).
  - `VacanteId` | `INT` | `NOT NULL` | Foránea (`FK_Matchings_Vacantes`).
  - `ScoreCoincidencia` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** | Porcentaje final de adecuación calculado por la IA.
  - `CoincidenciasText` | `NVARCHAR(MAX)` | `NOT NULL` | Resumen cualitativo de afinidades detectadas.
  - `BrechasText` | `NVARCHAR(MAX)` | `NOT NULL` | Resumen cualitativo de carencias del candidato.
  - `ExecutionId` | `BIGINT` | `NOT NULL` | Foránea (`FK_Matchings_AgentExecutions`) de auditoría de IA.
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
* **Constraints:**
  - `UQ_Matchings_Postulante_Vacante`: Unique Constraint en `(PostulanteId, VacanteId)`.

#### Tabla: `Scorings`
* **Nombre Físico:** `Scorings`
* **Columnas:**
  - `ScoringId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Scorings`).
  - `PostulanteId` | `INT` | `NOT NULL` | Foránea (`FK_Scorings_Postulantes`).
  - `VacanteId` | `INT` | `NOT NULL` | Foránea (`FK_Scorings_Vacantes`).
  - `ScoreSkills` | `INT` | `NOT NULL` | Puntuación parcial por habilidades.
  - `ScoreExperiencia` | `INT` | `NOT NULL` | Puntuación parcial por experiencia laboral.
  - `ScoreFinal` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** | Calificación final consolidada de adecuación (Always Encrypted).
  - `JustificacionText` | `NVARCHAR(MAX)` | `NOT NULL` | Explicación detallada de la calificación.
  - `ExecutionId` | `BIGINT` | `NOT NULL` | Foránea (`FK_Scorings_AgentExecutions`) de auditoría de IA.
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
* **Constraints:**
  - `UQ_Scorings_Postulante_Vacante`: Unique Constraint en `(PostulanteId, VacanteId)`.
  - `CK_Scorings_ScoreSkills`: Check Constraint: `(ScoreSkills BETWEEN 0 AND 100)`.
  - `CK_Scorings_ScoreExperiencia`: Check Constraint: `(ScoreExperiencia BETWEEN 0 AND 100)`.

---

### 2.6 Dominio: Coordinación de Entrevistas

#### Tabla: `Entrevistas`
* **Nombre Físico:** `Entrevistas`
* **Columnas:**
  - `EntrevistaId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Entrevistas`).
  - `VacanteId` | `INT` | `NOT NULL` | Foránea (`FK_Entrevistas_Vacantes`).
  - `PostulanteId` | `INT` | `NOT NULL` | Foránea (`FK_Entrevistas_Postulantes`).
  - `FechaHora` | `DATETIME2(7)` | `NOT NULL` | Horario agendado (UTC).
  - `TipoEntrevista` | `NVARCHAR(50)` | `NOT NULL` | Valores: `'Presencial'`, `'Teams'`, `'Llamada'`.
  - `TeamsJoinUrl` | `NVARCHAR(500)` | `NULL` | Enlace para la reunión virtual de Microsoft Teams.
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_Entrevistas_Estados`).
  - `ReprogramacionesContador` | `INT` | `NOT NULL` | Default: `0`. Máximo 3 admitidas.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `CK_Entrevistas_TipoEntrevista`: Check Constraint: `(TipoEntrevista IN ('Presencial', 'Teams', 'Llamada'))`.
  - `CK_Entrevistas_Reprogramaciones`: Check Constraint: `(ReprogramacionesContador <= 3)`.

---

### 2.7 Dominio: Gestión de Ofertas

#### Tabla: `Ofertas`
* **Nombre Físico:** `Ofertas`
* **Columnas:**
  - `OfertaId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Ofertas`).
  - `PostulanteId` | `INT` | `NOT NULL` | Foránea (`FK_Ofertas_Postulantes`).
  - `VacanteId` | `INT` | `NOT NULL` | Foránea (`FK_Ofertas_Vacantes`).
  - `BandaSalarialOfrecida` | `VARBINARY(max)` | `NOT NULL` | **Always Encrypted** | Sueldo exacto propuesto en el contrato.
  - `FechaEmision` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`.
  - `FechaExpiracion` | `DATETIME2(7)` | `NOT NULL` | Fecha límite para recibir la respuesta (SLA: 48 horas).
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_Ofertas_Estados`).
  - `JustificacionRechazo` | `NVARCHAR(500)` | `NULL` | Motivo de declinación por parte del candidato.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`

---

### 2.8 Dominio: SLAs y Gobernanza Operativa

#### Tabla: `SLAs`
* **Nombre Físico:** `SLAs`
* **Columnas:**
  - `SLAId` | `INT` | `NOT NULL` | Llave Primaria (`PK_SLAs`).
  - `Nombre` | `NVARCHAR(100)` | `NOT NULL` | Código de control (ej. `'SLA-SOL-03'`).
  - `DiasMaximos` | `INT` | `NOT NULL` | Límite permitido para resolver el estado.
  - `Modulo` | `NVARCHAR(50)` | `NOT NULL` | Nombre del módulo de negocio asignado.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_SLAs_Nombre`: Unique Constraint sobre `Nombre`.
  - `CK_SLAs_DiasMaximos`: Check Constraint: `(DiasMaximos > 0)`.

#### Tabla: `SLAExecutions`
* **Nombre Físico:** `SLAExecutions`
* **Columnas:**
  - `SLAExecutionId` | `BIGINT` | `NOT NULL` | Llave Primaria (`PK_SLAExecutions`).
  - `SLAId` | `INT` | `NOT NULL` | Foránea (`FK_SLAExecutions_SLAs`).
  - `Entidad` | `NVARCHAR(100)` | `NOT NULL` | Entidad evaluada (`'Solicitud'`, `'Vacante'`, `'Postulante'`).
  - `EntidadId` | `INT` | `NOT NULL` | Identificador del registro evaluado.
  - `EstadoId` | `INT` | `NOT NULL` | Foránea (`FK_SLAExecutions_Estados`).
  - `FechaInicio` | `DATETIME2(7)` | `NOT NULL` | UTC de entrada a la etapa.
  - `FechaLimite` | `DATETIME2(7)` | `NOT NULL` | UTC de expiración calculada (restando feriados y fines de semana).
  - `FechaFin` | `DATETIME2(7)` | `NULL` | UTC de salida de la etapa.
  - `Cumplido` | `BIT` | `NULL` | Default: `NULL` (En Proceso). Valores: `1` (Cumplido), `0` (Incumplido).
  - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL` | Enlace a la traza transaccional.
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `CK_SLAExecutions_Entidad`: Check Constraint: `(Entidad IN ('Solicitud', 'Vacante', 'Postulante'))`.

#### Tabla: `Feriados`
* **Nombre Físico:** `Feriados`
* **Columnas:**
  - `FeriadoId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Feriados`).
  - `Fecha` | `DATE` | `NOT NULL` | Fecha de calendario del feriado.
  - `Descripcion` | `NVARCHAR(150)` | `NOT NULL` | Nombre del feriado (ej: `'Año Nuevo'`).
  - `EsRecurrente` | `BIT` | `NOT NULL` | Default: `0`. Si es `1`, se repite anualmente el mismo día y mes.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `ModifiedBy` | `NVARCHAR(100)` | `NULL`
  - `ModifiedDate` | `DATETIME2(7)` | `NULL`
  - `DeletedBy` | `NVARCHAR(100)` | `NULL`
  - `DeletedDate` | `DATETIME2(7)` | `NULL`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Feriados_Fecha`: Unique Constraint sobre `Fecha` para evitar colisiones.

---

### 2.9 Dominio: Parametrizaciones e Integridad Jerárquica

#### Tabla: `Catalogos`
* **Nombre Físico:** `Catalogos`
* **Columnas:**
  - `CatalogoId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Catalogos`).
  - `Nombre` | `NVARCHAR(100)` | `NOT NULL` | Nombre descriptivo del catálogo.
  - `Codigo` | `NVARCHAR(50)` | `NOT NULL` | Código único de control.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Catalogos_Codigo`: Unique Constraint sobre `Codigo`.

#### Tabla: `Parametros`
* **Nombre Físico:** `Parametros`
* **Columnas:**
  - `ParametroId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Parametros`).
  - `CatalogoId` | `INT` | `NOT NULL` | Foránea (`FK_Parametros_Catalogos`).
  - `Codigo` | `NVARCHAR(50)` | `NOT NULL` | Código único por parámetro.
  - `Valor` | `NVARCHAR(250)` | `NOT NULL` | Valor descriptivo.
  - `ParametroIdPadre` | `INT` | `NULL` | Foránea auto-referencial (`FK_Parametros_Parametros_Padre`) para estructura en árbol.
  - `CreatedBy` | `NVARCHAR(100)` | `NOT NULL`
  - `CreatedDate` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
* **Constraints:**
  - `UQ_Parametros_Catalogo_Codigo`: Unique Constraint en `(CatalogoId, Codigo)`.
  - El trigger recursivo `trg_Parametro_PreventCircular` se asocia a esta tabla para revertir transacciones en caso de dependencias jerárquicas circulares.

#### Tabla: `Estados`
* **Nombre Físico:** `Estados`
* **Columnas:**
  - `EstadoId` | `INT` | `NOT NULL` | Llave Primaria (`PK_Estados`).
  - `Codigo` | `NVARCHAR(20)` | `NOT NULL` | Código abreviado del estado (ej. `'SOL-01'`).
  - `Nombre` | `NVARCHAR(50)` | `NOT NULL` | Nombre largo del estado (ej. `'Borrador'`).
  - `Entidad` | `NVARCHAR(50)` | `NOT NULL` | Entidad a la que aplica (`'Solicitud'`, `'Vacante'`, `'Postulante'`, `'Entrevista'`, `'Oferta'`).
  - `SLAId` | `INT` | `NULL` | Foránea (`FK_Estados_SLAs`).
* **Constraints:**
  - `UQ_Estados_Codigo`: Unique Constraint sobre `Codigo`.
  - `CK_Estados_Entidad`: Check Constraint: `(Entidad IN ('Solicitud', 'Vacante', 'Postulante', 'Entrevista', 'Oferta'))`.

---

## 3. Seguridad Física, Cifrado y RLS

La base de datos implementa las directrices de seguridad de Nacional Seguros e InterSIM para encriptación de datos confidenciales y control de accesos a nivel de filas:

### 3.1 Always Encrypted con Enclaves VBS
Las columnas que almacenan remuneración e idoneidad se configuran con **Always Encrypted** utilizando el driver del servidor de forma transparente:
* **Column Master Key (CMK):** Definida en el proveedor corporativo de gestión de secretos, configurando `KEY_STORE_PROVIDER_NAME = 'AZURE_KEY_VAULT'` y el identificador URI seguro de la clave.
* **Column Encryption Key (CEK):** Clave de cifrado de columna generada por el motor SQL y cifrada utilizando la CMK.
* **Tipo de Cifrado (Encryption Type):**
  - `BandaSalarialMin` | `VARBINARY(max)` | Cifrado Determinado (`DETERMINISTIC`) para permitir operaciones de igualdad en búsquedas.
  - `BandaSalarialMax` | `VARBINARY(max)` | Cifrado Determinado (`DETERMINISTIC`).
  - `PretensionSalarial` | `VARBINARY(max)` | Cifrado Aleatorio (`RANDOMIZED`) para evitar deducciones estadísticas de salario.
  - `ScoreCoincidencia` | `VARBINARY(max)` | Cifrado Determinado (`DETERMINISTIC`).
  - `ScoreFinal` | `VARBINARY(max)` | Cifrado Determinado (`DETERMINISTIC`).
  - `BandaSalarialOfrecida` | `VARBINARY(max)` | Cifrado Aleatorio (`RANDOMIZED`).
* **Enclave Criptográfico:** Se configuran **VBS Enclaves** en SQL Server 2022 para permitir operaciones de descifrado parcial del lado del servidor (como búsquedas de rango `BETWEEN` en salarios) de manera segura en memoria protegida.

### 3.2 Row Level Security (RLS) - Seguridad a nivel de filas
Para la tabla `Solicitudes` y `Vacantes`, se define una política de RLS para evitar que usuarios de un departamento visualicen solicitudes de otro área:
* **Función de Predicado:** `fn_SecurityPredicateArea(Area)`
  - Retorna `1` si el usuario en sesión (`SESSION_CONTEXT('UserArea')`) coincide con el área del registro, o si el rol del usuario conectado (`SESSION_CONTEXT('UserRol')`) es `'Administrador'`, `'RRHH'` o `'Auditor'`. En caso contrario retorna `0`.
* **Política de Seguridad:** `SecurityPolicyArea`
  - Aplica un filtro (`FILTER PREDICATE`) y un bloqueo de inserción/actualización (`BLOCK PREDICATE`) sobre las tablas `Solicitudes` y `Vacantes` utilizando la función de predicado.

### 3.3 Clasificación de Datos (Data Classification)
Para auditorías externas de cumplimiento, se aplican metadatos de clasificación a nivel de columna:
* **PII (Información de Identificación Personal):** `Postulantes.Nombres`, `Postulantes.Apellidos`, `Postulantes.Correo`, `Postulantes.DocumentoIdentidad`.
* **Financiero / Confidencialidad Alta:** `Vacantes.BandaSalarialMin`, `Vacantes.BandaSalarialMax`, `Postulantes.PretensionSalarial`, `Ofertas.BandaSalarialOfrecida`.

---

## 4. Auditoría, Inmutabilidad y Particionamiento (SQL Server Ledger)

### 4.1 SQL Server Ledger (Tablas Inmutables)
Para cumplir con la inmutabilidad física exigida, las tablas de logs y estados se configuran en SQL Server 2022 con la cláusula **`LEDGER = ON (APPEND_ONLY = ON)`**:

1. **`AuditLogs`:** Almacena todos los cambios relacionales generados por EF Core. Al ser `APPEND_ONLY = ON`, el motor bloquea cualquier comando `UPDATE` o `DELETE` y genera un histórico hash encadenado criptográficamente en el sistema.
2. **`StateHistory` (Historial de Estados):**
   - **Nombre Físico:** `StateHistory` (Ledger)
   - **Columnas:**
     - `StateHistoryId` | `BIGINT` | `NOT NULL` | Llave Primaria
     - `Entidad` | `NVARCHAR(100)` | `NOT NULL`
     - `EntidadId` | `INT` | `NOT NULL`
     - `EstadoAnteriorId` | `INT` | `NULL` | Foránea (`FK_StateHistory_Estados_Anterior`)
     - `EstadoNuevoId` | `INT` | `NOT NULL` | Foránea (`FK_StateHistory_Estados_Nuevo`)
     - `UsuarioId` | `INT` | `NOT NULL` | Foránea (`FK_StateHistory_Usuarios`)
     - `Fecha` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
     - `Comentario` | `NVARCHAR(500)` | `NULL` | Justificación del cambio de estado.
     - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL`
3. **`AgentExecutions` (Gobernanza de IA):**
   - **Nombre Físico:** `AgentExecutions` (Ledger)
   - **Columnas:**
     - `ExecutionId` | `BIGINT` | `NOT NULL` | Llave Primaria
     - `AgenteId` | `INT` | `NOT NULL` | Foránea (`FK_AgentExecutions_Parametros_Agente`)
     - `PromptVersionId` | `INT` | `NOT NULL` | Foránea (`FK_AgentExecutions_PromptVersions`)
     - `UsuarioId` | `INT` | `NOT NULL` | Foránea (`FK_AgentExecutions_Usuarios`)
     - `FechaInicio` | `DATETIME2(7)` | `NOT NULL`
     - `FechaFin` | `DATETIME2(7)` | `NOT NULL`
     - `DuracionMs` | `INT` | `NOT NULL` | Diferencia entre inicio y fin.
     - `InputJson` | `NVARCHAR(MAX)` | `NOT NULL` | Entrada anonimizada.
     - `OutputJson` | `NVARCHAR(MAX)` | `NOT NULL` | Respuesta de la IA.
     - `ResultadoStatus` | `NVARCHAR(30)` | `NOT NULL` | Valores: `'Exito'`, `'Fallo'`, `'Timeout'`.
     - `TokensInput` | `INT` | `NOT NULL`
     - `TokensOutput` | `INT` | `NOT NULL`
     - `CostoEstimado` | `DECIMAL(10,5)` | `NOT NULL` | Costo acumulado en USD.
     - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL`

### 4.2 Estrategia de Particionamiento
Para evitar la degradación del rendimiento en la base de datos operativa, las tablas Ledger de auditoría que acumulan millones de filas anuales se configuran bajo un esquema de **Particionamiento por Rango de Fechas**:
* **Función de Partición:** `pf_FechaMensual`
  - Particiona los datos basándose en la columna de fecha (`CreatedDate` o `FechaInicio`) de forma mensual utilizando `RANGE RIGHT`.
* **Esquema de Partición:** `ps_FechaMensual`
  - Distribuye las particiones en Filegroups independientes (`FG_SIR_Audit_2026_01`, `FG_SIR_Audit_2026_02`, etc.).
* **Tablas Particionadas:** `AuditLogs`, `StateHistory` y `AgentExecutions`.
* **Archivado (Data Lifecycle):**
  - **Hot Data (Filegroup Principal):** Datos de los últimos 12 meses.
  - **Cold Data (Filegroups históricos):** Datos de más de 12 meses se migran a almacenamiento frío inmutable o se realiza un Switch de partición a una base de datos histórica comprimida.

---

## 5. Mapeo y Estrategia de Soft Delete

Para mantener la integridad referencial sin perder registros históricos, el sistema utiliza un borrado lógico transversal controlado mediante Entity Framework Core:

* **Columnas Físicas por Tabla:**
  - `IsDeleted` | `BIT` | `NOT NULL` | Default: `0`
  - `DeletedDate` | `DATETIME2(7)` | `NULL`
  - `DeletedBy` | `NVARCHAR(100)` | `NULL`
* **Estrategia Global EF Core (`HasQueryFilter`):**
  - Las entidades mapeadas en .NET que contengan la propiedad de borrado lógico configuran un filtro global en el método `OnModelCreating` de EF:
    ```csharp
    modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.IsDeleted);
    modelBuilder.Entity<Solicitud>().HasQueryFilter(s => !s.IsDeleted);
    modelBuilder.Entity<Vacante>().HasQueryFilter(v => !v.IsDeleted);
    modelBuilder.Entity<Postulante>().HasQueryFilter(p => !p.IsDeleted);
    ```
  - **Búsqueda Administrativa:** Si un usuario auditor requiere ver datos borrados, el query del backend utiliza explícitamente `.IgnoreQueryFilters()` para evadir el filtrado y retornar todos los registros de la tabla física.

---

## 6. Dominio: Observabilidad y Logs de Integración

#### Tabla: `IntegrationLogs`
* **Nombre Físico:** `IntegrationLogs` (Ledger)
* **Columnas:**
  - `IntegrationLogId` | `BIGINT` | `NOT NULL` | Llave Primaria
  - `Fecha` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `SistemaExterno` | `NVARCHAR(100)` | `NOT NULL` | Valores: `'MicrosoftGraph'`, `'WhatsAppBusiness'`, `'LinkedIn'`.
  - `EndpointUrl` | `NVARCHAR(250)` | `NOT NULL`
  - `MetodoHttp` | `NVARCHAR(10)` | `NOT NULL`
  - `Resultado` | `NVARCHAR(30)` | `NOT NULL` | Valores: `'Exitoso'`, `'Fallido'`, `'Reintento'`.
  - `ErrorMessage` | `NVARCHAR(MAX)` | `NULL` | Traza del error sanitizada.
  - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL`

#### Tabla: `WorkflowExecutions`
* **Nombre Físico:** `WorkflowExecutions`
* **Columnas:**
  - `WorkflowExecutionId` | `BIGINT` | `NOT NULL` | Llave Primaria
  - `WorkflowName` | `NVARCHAR(100)` | `NOT NULL` | Código (ej. `'WF-05-MatchingCurricular'`).
  - `FechaInicio` | `DATETIME2(7)` | `NOT NULL`
  - `FechaFin` | `DATETIME2(7)` | `NULL`
  - `EstadoExecution` | `NVARCHAR(30)` | `NOT NULL` | Valores: `'Corriendo'`, `'Completado'`, `'Fallido'`.
  - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL`

#### Tabla: `NotificationLogs`
* **Nombre Físico:** `NotificationLogs`
* **Columnas:**
  - `NotificationLogId` | `BIGINT` | `NOT NULL` | Llave Primaria
  - `Fecha` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`
  - `Destinatario` | `NVARCHAR(150)` | `NOT NULL` | Correo o número de teléfono.
  - `TipoCanal` | `NVARCHAR(20)` | `NOT NULL` | Valores: `'Email'`, `'WhatsApp'`, `'Teams'`.
  - `Asunto` | `NVARCHAR(200)` | `NULL`
  - `EstadoEnvio` | `NVARCHAR(30)` | `NOT NULL` | Valores: `'Enviado'`, `'Fallido'`.
  - `CorrelationId` | `UNIQUEIDENTIFIER` | `NOT NULL`

#### Tabla: `MetricSnapshot`
* **Nombre Físico:** `MetricSnapshot`
* **Columnas:**
  - `MetricSnapshotId` | `BIGINT` | `NOT NULL` | Llave Primaria (`PK_MetricSnapshot`) | Identificador único incremental.
  - `MetricaNombre` | `NVARCHAR(100)` | `NOT NULL` | Nombre descriptivo de la métrica (ej. `'SLA_CostoTokensAcumuladoUsd'`, `'SLA_CumplimientoPorcentaje'`, `'TotalPostulacionesPorOrigen'`).
  - `FechaHora` | `DATETIME2(7)` | `NOT NULL` | Default: `GETUTCDATE()`. Fecha y hora de generación de la métrica.
  - `Valor` | `DECIMAL(18,4)` | `NOT NULL` | Valor de la métrica consolidada.
  - `AgrupacionClave` | `NVARCHAR(100)` | `NULL` | Clave de agrupación/segmento si aplica (ej. `'Modulo'`, `'Origen'`).
  - `AgrupacionValor` | `NVARCHAR(250)` | `NULL` | Valor de la agrupación/segmento (ej. `'Solicitudes'`, `'LinkedIn'`).

---

## 7. Estrategia de Índices (Index Strategy)

Para acelerar la velocidad de consulta y paginación en la API, se definen los siguientes índices físicos justificados en base a los patrones de acceso del sistema:

1. **Clustered Indexes (Índices Agrupados):**
   - Configurados por defecto sobre la clave primaria (`Id` o `[Entidad]Id`) autoincremental de cada tabla física, optimizando la ordenación en disco.
2. **Non-Clustered Indexes (NCI):**
   - **`IX_Postulantes_Documento`:** En `Postulantes(DocumentoIdentidad)`. Justificación: Búsqueda y validación de unicidad de candidatos en el registro.
   - **`IX_SLAExecutions_Entidad_Id`:** En `SLAExecutions(Entidad, EntidadId)`. Justificación: Acelera la carga de tiempos de SLA asociados a una solicitud o vacante.
   - **`IX_StateHistory_Entidad_Id`:** En `StateHistory(Entidad, EntidadId)`. Justificación: Acelera la generación visual de la línea de tiempo.
3. **Covering Indexes (Índices de Cobertura):**
   - **`IX_Usuarios_Active_Cover`:** En `Usuarios(Correo)` incluyendo `(Nombre, Estado, TipoAutenticacion)`. Justificación: El Handler de login busca por correo; el índice permite retornar los datos básicos de sesión de forma instantánea sin ir a la tabla de datos física.
4. **Filtered Indexes (Índices Filtrados):**
   - **`FIX_Usuarios_AD`:** En `Usuarios(ActiveDirectoryId)` donde `ActiveDirectoryId IS NOT NULL`. Justificación: Acelera la sincronización y login de usuarios corporativos sin escanear usuarios locales.
   - **`FIX_Solicitudes_Activas`:** En `Solicitudes(SolicitudId)` donde `IsDeleted = 0`. Justificación: Optimiza el listado del Kanban de solicitudes activas.
5. **Full Text Search (Búsqueda de Texto Completo):**
   - Configurado en `PerfilesCargo(Descripcion)` y `Solicitudes(Funciones, Skills)`. Justificación: Permite al Agente de Matching realizar búsquedas semánticas rápidas por palabras clave en los perfiles y solicitudes sin sobrecargar consultas `LIKE '%text%'`.

---

## 8. Catálogo de Vistas Físicas

#### 1. `vw_DashboardEjecutivo`
* **Propósito:** Consolidar métricas globales para la alta dirección de Nacional Seguros.
* **Campos:** `TotalSolicitudes`, `TotalVacantesActivas`, `TotalPostulantes`, `TiempoPromedioCoberturaDias`, `CostoTokensAcumuladoUsd`.
* **Fuente:** `Solicitudes`, `Vacantes`, `Postulantes`, `AgentExecutions`.

#### 2. `vw_DashboardSLA`
* **Propósito:** Mostrar el porcentaje de cumplimiento y desvíos de SLAs.
* **Campos:** `SLAExecutionId`, `NombreSLA`, `Modulo`, `FechaInicio`, `FechaLimite`, `TranscurridoHoras`, `EstadoAlerta` (`'Verde'`, `'Amarillo'`, `'Naranja'`, `'Rojo'`), `ResponsableAsignado`.
* **Fuente:** `SLAExecutions`, `SLAs`, `Estados`, `Usuarios`.

#### 3. `vw_DashboardReclutamiento`
* **Propósito:** Panel operativo para analistas de RRHH.
* **Campos:** `VacanteId`, `Cargo`, `Area`, `DiasAbierta`, `CantidadCandidatos`, `CandidatosEnTerna`, `SLAEstado`.
* **Fuente:** `Vacantes`, `PerfilesCargo`, `Postulaciones`, `Postulantes`.

#### 4. `vw_DashboardIA`
* **Propósito:** Monitorear la eficiencia y costos de tokens de IA por agente.
* **Campos:** `AgenteNombre`, `VersionPrompt`, `TotalEjecuciones`, `TokensInputPromedio`, `TokensOutputPromedio`, `CostoTotalUsd`, `LatenciaPromedioMs`.
* **Fuente:** `AgentExecutions`, `PromptVersions`.

#### 5. `vw_PostulantePipeline`
* **Propósito:** Proveer la estructura lógica para renderizar el Kanban de postulantes en el frontend.
* **Campos:** `PostulanteId`, `NombreCompleto`, `VacanteId`, `Cargo`, `EstadoPipelineId`, `EstadoPipelineCodigo`, `ScoreFinal`, `ColorSLA`.
* **Fuente:** `Postulaciones`, `Postulantes`, `Vacantes`, `Scorings`, `Estados`.

---

## 9. Catálogo de Procedimientos Almacenados (Stored Procedures)

Para encapsular la lógica de negocio compleja, proteger los datos contra inyecciones SQL y optimizar el rendimiento, se define el siguiente catálogo de Stored Procedures (sin código T-SQL ejecutable en las definiciones):

### 9.1 Módulo de Seguridad
* **`sp_Seguridad_RegistrarSesion`:** Registra la emisión de un nuevo refresh token en `Sesiones`.
  - *Parámetros:* `@UsuarioId INT`, `@RefreshToken NVARCHAR(256)`, `@FechaExpiracion DATETIME2(7)`.
* **`sp_Seguridad_InvalidarSesionesUsuario`:** Anula todas las sesiones activas de un usuario en caso de detección de reuso de tokens (RTR).
  - *Parámetros:* `@UsuarioId INT`, `@CorrelationId UNIQUEIDENTIFIER`.

### 9.2 Módulo de Reclutamiento y Estados
* **`sp_Reclutamiento_TransitarEstadoPostulacion`:** Ejecuta la transición de una postulación en el pipeline, escribiendo de forma atómica en `Postulaciones` y en la tabla Ledger `StateHistory`.
  - *Parámetros:* `@PostulacionId INT`, `@NuevoEstadoId INT`, `@UsuarioId INT`, `@Comentario NVARCHAR(500)`, `@CorrelationId UNIQUEIDENTIFIER`.

### 9.3 Módulo de SLAs
* **`sp_SLA_CalcularFechaLimite`:** Calcula la fecha límite de un SLA restando fines de semana y las fechas cargadas en la tabla `Feriados`.
  - *Parámetros:* `@SLAId INT`, `@FechaInicio DATETIME2(7)`, `@FechaLimiteCalculada DATETIME2(7) OUTPUT`.

### 9.4 Módulo de Reportería y Auditoría
* **`sp_Reporte_CargarSnapshotMetricas`:** Ejecuta semanalmente la agregación de métricas de cobertura y costos de IA para poblar `MetricSnapshot`.
  - *Parámetros:* `@FechaReferencia DATE`.
* **`sp_Auditoria_ConsultarEjecucionesAgente`:** Permite al rol `Auditor` extraer logs de la tabla Ledger `AgentExecutions` filtrando por correlación o agente.
  - *Parámetros:* `@AgenteId INT`, `@CorrelationId UNIQUEIDENTIFIER`.

---

## 10. Rendimiento y Plan de Mantenimiento

* **Query Store (Almacén de Consultas):**
  - Configurado en modo `READ_WRITE` de forma mandatoria.
  - Permite identificar de manera inmediata regresiones en los planes de ejecución provocadas por actualizaciones de EF Core y forzar planes estables óptimos.
* **Compresión de Datos (Data Compression):**
  - Las tablas Ledger e históricas (`AuditLogs`, `StateHistory`, `AgentExecutions`, `IntegrationLogs`) aplican compresión de datos a nivel de **Página (PAGE)** para reducir hasta un 50% el espacio ocupado en disco y acelerar la velocidad de lectura física de I/O.
* **Mantenimiento Periódico (Job SQL Agent):**
  - **Diario (Fuera de oficina):** Actualización de estadísticas (`UPDATE STATISTICS`) con muestreo completo en índices de tablas altamente transaccionales.
  - **Semanal (Fin de semana):** Reorganización de índices si la fragmentación es mayor a 10%, y reconstrucción completa (`REBUILD`) si la fragmentación supera el 30%.

---

## 11. Políticas de Backup y Recuperación ante Desastres (DRP)

Para cumplir con las políticas corporativas de continuidad de negocio de Nacional Seguros, se definen los siguientes esquemas de respaldos inmutables sobre el servidor SQL Server 2022:

* **Estrategia de Respaldos:**
  - **Backup Completo (Full):** Ejecutado de forma semanal (domingos a las 01:00 AM UTC).
  - **Backup Diferencial (Diff):** Ejecutado de forma diaria (lunes a sábados a las 02:00 AM UTC).
  - **Backup de Registros de Transacción (Log):** Ejecutado cada 15 minutos de forma ininterrumpida para garantizar recuperación en un punto del tiempo (Point-in-Time Recovery).
* **Parámetros de Continuidad:**
  - **RPO (Punto Objetivo de Recuperación):** Máximo **15 minutos** de pérdida de datos.
  - **RTO (Tiempo Objetivo de Recuperación):** Máximo **2 horas** para restaurar la operatividad total de la base de datos en una región o servidor de contingencia.
* **Resguardo de Backups:** Los archivos de respaldo se copian de forma cifrada a un almacenamiento redundante inmutable geodistribuido.

---

## 12. Riesgos Técnicos y Mitigaciones (SQL Server)

| ID | Riesgo Detectado | Severidad | Impacto | Control y Mitigación Propuesta |
| :---: | :--- | :---: | :--- | :--- |
| **R-SQL-01** | **Bloqueo en el Trigger de Dependencia Circular** | 🔴 Alto | Locks extensos y timeouts en operaciones simultáneas sobre la tabla `Parametros`. | Implementar validación defensiva en C# en la capa de aplicación antes de guardar, dejando el trigger de BD solo como control redundante final. |
| **R-SQL-02** | **Degradación de Always Encrypted en Enclaves** | 🟠 Medio | Alto uso de CPU en el servidor de base de datos debido a descifrados masivos en memoria segura. | Limitar la encriptación estrictamente a las 6 columnas sensibles aprobadas. Evitar JOINs sobre columnas cifradas. |
| **R-SQL-03** | **Crecimiento Excesivo del Log de Transacciones** | 🟠 Medio | Llenado de disco del servidor SQL provocado por la alta tasa de logs de auditoría inmutables Ledger. | Configurar el archivado mensual y compresión PAGE de las particiones de la tabla `AuditLogs`. |
| **R-SQL-04** | **Pérdida de Llaves Criptográficas (Always Encrypted)** | 🔴 Alto | Pérdida permanente del acceso a datos salariales por eliminación accidental de la CMK. | Habilitar Soft-Delete y protección contra purga en el proveedor corporativo de gestión de secretos. Respaldos periódicos de la CMK con los mecanismos de respaldo del proveedor. |

---

## 13. Recomendaciones Técnicas de Implementación

1. **Uso de SQL Server Profiler / Extended Events en Pruebas:** Durante el desarrollo de la capa de persistencia en EF Core 9, utilizar Extended Events para monitorear que las consultas complejas no fuercen escaneos de tablas completas (Table Scans) en lugar de búsquedas por índice (Index Seeks).
2. **Configuración de Max Degree of Parallelism (MAXDOP):** Configurar MAXDOP a un valor de 4 u 8 en base al número de núcleos físicos del servidor SQL, para evitar que una única query de analítica consuma todos los hilos del procesador.
3. **Caché en Memoria de Feriados:** Para evitar llamadas redundantes a la tabla `Feriados` en cada guardado de SLA, el backend de .NET debe almacenar estas fechas en una caché local inmutable con vencimiento de 24 horas.
