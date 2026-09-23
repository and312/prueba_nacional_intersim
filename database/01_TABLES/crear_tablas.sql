-- ==========================================
-- 01_TABLES/crear_tablas.sql
-- Creación de Tablas del Sistema (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- 1. Dominio: Seguridad
CREATE TABLE Roles (
    RolId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Roles_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (RolId)
);

CREATE TABLE Permisos (
    PermisoId INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Permisos_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Permisos_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Permisos PRIMARY KEY CLUSTERED (PermisoId)
);

CREATE TABLE RolPermisos (
    RolId INT NOT NULL,
    PermisoId INT NOT NULL,
    CONSTRAINT PK_RolPermisos PRIMARY KEY CLUSTERED (RolId, PermisoId)
);

CREATE TABLE Usuarios (
    UsuarioId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Correo NVARCHAR(100) NOT NULL,
    ClaveHash NVARCHAR(256) NULL,
    TipoAutenticacion NVARCHAR(30) NOT NULL CONSTRAINT DF_Usuarios_TipoAuth DEFAULT 'Local',
    ActiveDirectoryId NVARCHAR(100) NULL,
    Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT 'Activo',
    MfaHabilitado BIT NOT NULL CONSTRAINT DF_Usuarios_MfaHabilitado DEFAULT 0,
    MfaSecreto NVARCHAR(128) NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Usuarios_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    DeletedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Usuarios_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (UsuarioId)
);

CREATE TABLE UsuarioRoles (
    UsuarioId INT NOT NULL,
    RolId INT NOT NULL,
    CONSTRAINT PK_UsuarioRoles PRIMARY KEY CLUSTERED (UsuarioId, RolId)
);

CREATE TABLE Sesiones (
    SesionId BIGINT IDENTITY(1,1) NOT NULL,
    UsuarioId INT NOT NULL,
    RefreshToken NVARCHAR(256) NOT NULL,
    FechaExpiracion DATETIME2(7) NOT NULL,
    Activa BIT NOT NULL CONSTRAINT DF_Sesiones_Activa DEFAULT 1,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Sesiones_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Sesiones PRIMARY KEY CLUSTERED (SesionId)
);

-- 2. Dominio: Parametrización e Integridad Jerárquica
CREATE TABLE SLAs (
    SLAId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    DiasMaximos INT NOT NULL,
    Modulo NVARCHAR(50) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_SLAs_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_SLAs_IsDeleted DEFAULT 0,
    CONSTRAINT PK_SLAs PRIMARY KEY CLUSTERED (SLAId)
);

CREATE TABLE Catalogos (
    CatalogoId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Catalogos_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Catalogos_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Catalogos PRIMARY KEY CLUSTERED (CatalogoId)
);

CREATE TABLE Parametros (
    ParametroId INT IDENTITY(1,1) NOT NULL,
    CatalogoId INT NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Valor NVARCHAR(250) NOT NULL,
    ParametroIdPadre INT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Parametros_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Parametros_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Parametros PRIMARY KEY CLUSTERED (ParametroId)
);

CREATE TABLE Estados (
    EstadoId INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(50) NOT NULL,
    Entidad NVARCHAR(50) NOT NULL,
    SLAId INT NULL,
    CONSTRAINT PK_Estados PRIMARY KEY CLUSTERED (EstadoId)
);

CREATE TABLE Feriados (
    FeriadoId INT IDENTITY(1,1) NOT NULL,
    Fecha DATE NOT NULL,
    Descripcion NVARCHAR(150) NOT NULL,
    EsRecurrente BIT NOT NULL CONSTRAINT DF_Feriados_EsRecurrente DEFAULT 0,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Feriados_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    DeletedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Feriados_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Feriados PRIMARY KEY CLUSTERED (FeriadoId)
);

-- 3. Dominio: Solicitudes de Personal
CREATE TABLE Solicitudes (
    SolicitudId INT IDENTITY(1,1) NOT NULL,
    Cargo NVARCHAR(100) NOT NULL,
    Area NVARCHAR(100) NOT NULL,
    SolicitanteId INT NOT NULL,
    DecisorId INT NULL,
    Modalidad NVARCHAR(50) NOT NULL,
    Seniority NVARCHAR(50) NOT NULL,
    Prioridad NVARCHAR(20) NOT NULL,
    FechaIdeal DATE NOT NULL,
    Funciones NVARCHAR(MAX) NOT NULL,
    Skills NVARCHAR(MAX) NOT NULL,
    EstadoId INT NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Solicitudes_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    DeletedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Solicitudes_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Solicitudes PRIMARY KEY CLUSTERED (SolicitudId)
);

CREATE TABLE SolicitudComentarios (
    ComentarioId INT IDENTITY(1,1) NOT NULL,
    SolicitudId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Texto NVARCHAR(1000) NOT NULL,
    Fecha DATETIME2(7) NOT NULL CONSTRAINT DF_SolicitudComentarios_Fecha DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SolicitudComentarios PRIMARY KEY CLUSTERED (ComentarioId)
);

-- 4. Dominio: Perfiles de Cargo (Profesiogramas)
CREATE TABLE PerfilesCargo (
    PerfilCargoId INT IDENTITY(1,1) NOT NULL,
    SolicitudId INT NOT NULL,
    Cargo NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    Version INT NOT NULL CONSTRAINT DF_PerfilesCargo_Version DEFAULT 1,
    EstadoId INT NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_PerfilesCargo_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PerfilesCargo_IsDeleted DEFAULT 0,
    CONSTRAINT PK_PerfilesCargo PRIMARY KEY CLUSTERED (PerfilCargoId)
);

-- 5. Dominio: Gestión de Vacantes
CREATE TABLE Vacantes (
    VacanteId INT IDENTITY(1,1) NOT NULL,
    PerfilCargoId INT NOT NULL,
    SolicitudId INT NOT NULL,
    EstadoId INT NOT NULL,
    FechaApertura DATETIME2(7) NOT NULL CONSTRAINT DF_Vacantes_FechaApertura DEFAULT GETUTCDATE(),
    FechaCierre DATETIME2(7) NULL,
    BandaSalarialMin DECIMAL(18,2) NOT NULL, -- Logical type
    BandaSalarialMax DECIMAL(18,2) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Vacantes_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Vacantes_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Vacantes PRIMARY KEY CLUSTERED (VacanteId)
);

-- 6. Dominio: Gestión de Postulantes y Postulaciones (Normalizado M:N)
CREATE TABLE Postulantes (
    PostulanteId INT IDENTITY(1,1) NOT NULL,
    Nombres NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    Correo NVARCHAR(100) NOT NULL,
    DocumentoIdentidad NVARCHAR(30) NOT NULL,
    Origen NVARCHAR(50) NOT NULL CONSTRAINT DF_Postulantes_Origen DEFAULT 'LinkedIn',
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Postulantes_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Postulantes_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Postulantes PRIMARY KEY CLUSTERED (PostulanteId)
);

CREATE TABLE Postulaciones (
    PostulacionId INT IDENTITY(1,1) NOT NULL,
    PostulanteId INT NOT NULL,
    VacanteId INT NOT NULL,
    FechaPostulacion DATETIME2(7) NOT NULL CONSTRAINT DF_Postulaciones_Fecha DEFAULT GETUTCDATE(),
    EstadoPipelineId INT NOT NULL,
    PretensionSalarial DECIMAL(18,2) NOT NULL, -- Logical type
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Postulaciones_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Postulaciones_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Postulaciones PRIMARY KEY CLUSTERED (PostulacionId)
);

CREATE TABLE Matchings (
    MatchingId INT IDENTITY(1,1) NOT NULL,
    PostulanteId INT NOT NULL,
    VacanteId INT NOT NULL,
    ScoreCoincidencia DECIMAL(5,2) NOT NULL, -- Logical type
    CoincidenciasText NVARCHAR(MAX) NOT NULL,
    BrechasText NVARCHAR(MAX) NOT NULL,
    ExecutionId BIGINT NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Matchings_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Matchings PRIMARY KEY CLUSTERED (MatchingId)
);

CREATE TABLE Scorings (
    ScoringId INT IDENTITY(1,1) NOT NULL,
    PostulanteId INT NOT NULL,
    VacanteId INT NOT NULL,
    ScoreSkills INT NOT NULL,
    ScoreExperiencia INT NOT NULL,
    ScoreFinal DECIMAL(5,2) NOT NULL, -- Logical type
    JustificacionText NVARCHAR(MAX) NOT NULL,
    ExecutionId BIGINT NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Scorings_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Scorings PRIMARY KEY CLUSTERED (ScoringId)
);

-- 7. Dominio: Coordinación de Entrevistas
CREATE TABLE Entrevistas (
    EntrevistaId INT IDENTITY(1,1) NOT NULL,
    VacanteId INT NOT NULL,
    PostulanteId INT NOT NULL,
    FechaHora DATETIME2(7) NOT NULL,
    TipoEntrevista NVARCHAR(50) NOT NULL,
    TeamsJoinUrl NVARCHAR(500) NULL,
    EstadoId INT NOT NULL,
    ReprogramacionesContador INT NOT NULL CONSTRAINT DF_Entrevistas_Reprog DEFAULT 0,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Entrevistas_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Entrevistas_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Entrevistas PRIMARY KEY CLUSTERED (EntrevistaId)
);

-- 8. Dominio: Gestión de Ofertas y Contrataciones
CREATE TABLE Ofertas (
    OfertaId INT IDENTITY(1,1) NOT NULL,
    PostulanteId INT NOT NULL,
    VacanteId INT NOT NULL,
    BandaSalarialOfrecida DECIMAL(18,2) NOT NULL, -- Logical type
    FechaEmision DATETIME2(7) NOT NULL CONSTRAINT DF_Ofertas_FechaEmision DEFAULT GETUTCDATE(),
    FechaExpiracion DATETIME2(7) NOT NULL,
    EstadoId INT NOT NULL,
    JustificacionRechazo NVARCHAR(500) NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Ofertas_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Ofertas_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Ofertas PRIMARY KEY CLUSTERED (OfertaId)
);

CREATE TABLE Contrataciones (
    ContratacionId INT IDENTITY(1,1) NOT NULL,
    PostulanteId INT NOT NULL,
    VacanteId INT NOT NULL,
    FechaEfectivaIngreso DATE NOT NULL,
    TipoContratacion NVARCHAR(50) NOT NULL,
    Estado NVARCHAR(50) NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Contrataciones_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Contrataciones_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Contrataciones PRIMARY KEY CLUSTERED (ContratacionId)
);

-- 9. Dominio: Inteligencia Artificial (IA)
CREATE TABLE Agentes (
    AgenteId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Agentes_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Agentes_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Agentes PRIMARY KEY CLUSTERED (AgenteId)
);

CREATE TABLE Prompts (
    PromptId INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Prompts_Activo DEFAULT 1,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Prompts_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Prompts_IsDeleted DEFAULT 0,
    CONSTRAINT PK_Prompts PRIMARY KEY CLUSTERED (PromptId)
);

CREATE TABLE PromptVersions (
    PromptVersionId INT IDENTITY(1,1) NOT NULL,
    PromptId INT NOT NULL,
    VersionNumber INT NOT NULL,
    PromptSystem NVARCHAR(MAX) NOT NULL,
    PromptUser NVARCHAR(MAX) NOT NULL,
    Variables NVARCHAR(500) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_PromptVersions_Activo DEFAULT 0,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_PromptVersions_CreatedDate DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL CONSTRAINT DF_PromptVersions_IsDeleted DEFAULT 0,
    CONSTRAINT PK_PromptVersions PRIMARY KEY CLUSTERED (PromptVersionId)
);

CREATE TABLE ModelExecutions (
    ModelExecutionId BIGINT IDENTITY(1,1) NOT NULL,
    ExecutionId BIGINT NOT NULL,
    ModelName NVARCHAR(100) NOT NULL,
    Provider NVARCHAR(50) NOT NULL,
    PromptTokens INT NOT NULL,
    CompletionTokens INT NOT NULL,
    TotalTokens INT NOT NULL,
    ExecutionTimeMs INT NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_ModelExecutions_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ModelExecutions PRIMARY KEY CLUSTERED (ModelExecutionId)
);

CREATE TABLE TokenConsumptions (
    TokenConsumptionId BIGINT IDENTITY(1,1) NOT NULL,
    ExecutionId BIGINT NOT NULL,
    ConsumerType NVARCHAR(50) NOT NULL,
    ModelName NVARCHAR(100) NOT NULL,
    TokensInput INT NOT NULL,
    TokensOutput INT NOT NULL,
    CostoUSD DECIMAL(10,5) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_TokenConsumptions_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TokenConsumptions PRIMARY KEY CLUSTERED (TokenConsumptionId)
);

CREATE TABLE CostTrackings (
    CostTrackingId INT IDENTITY(1,1) NOT NULL,
    Periodo NVARCHAR(7) NOT NULL,
    Concepto NVARCHAR(100) NOT NULL,
    TokensConsumidos BIGINT NOT NULL,
    CostoTotalUSD DECIMAL(12,5) NOT NULL,
    LimiteAlertaUSD DECIMAL(10,2) NOT NULL,
    AlertaDisparada BIT NOT NULL CONSTRAINT DF_CostTrackings_Alerta DEFAULT 0,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_CostTrackings_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_CostTrackings PRIMARY KEY CLUSTERED (CostTrackingId)
);

-- 10. Dominio: SLAs y Gobernanza de Alertas
CREATE TABLE SLAAlerts (
    SLAAlertId INT IDENTITY(1,1) NOT NULL,
    SLAExecutionId BIGINT NOT NULL,
    NivelAlerta NVARCHAR(20) NOT NULL,
    FechaAlerta DATETIME2(7) NOT NULL CONSTRAINT DF_SLAAlerts_Fecha DEFAULT GETUTCDATE(),
    Destinatario NVARCHAR(100) NOT NULL,
    Enviado BIT NOT NULL CONSTRAINT DF_SLAAlerts_Enviado DEFAULT 0,
    CONSTRAINT PK_SLAAlerts PRIMARY KEY CLUSTERED (SLAAlertId)
);

CREATE TABLE SLAEscalations (
    SLAEscalationId INT IDENTITY(1,1) NOT NULL,
    SLAExecutionId BIGINT NOT NULL,
    NivelEscalacion INT NOT NULL,
    ResponsableOriginalId INT NOT NULL,
    ResponsableNuevoId INT NOT NULL,
    FechaEscalacion DATETIME2(7) NOT NULL CONSTRAINT DF_SLAEscalations_Fecha DEFAULT GETUTCDATE(),
    Comentario NVARCHAR(500) NULL,
    CONSTRAINT PK_SLAEscalations PRIMARY KEY CLUSTERED (SLAEscalationId)
);

-- 11. Dominio: Observabilidad y Logs de Integración
CREATE TABLE IntegrationLogs (
    IntegrationLogId BIGINT IDENTITY(1,1) NOT NULL,
    Fecha DATETIME2(7) NOT NULL CONSTRAINT DF_IntegrationLogs_Fecha DEFAULT GETUTCDATE(),
    SistemaExterno NVARCHAR(100) NOT NULL,
    EndpointUrl NVARCHAR(250) NOT NULL,
    MetodoHttp NVARCHAR(10) NOT NULL,
    Resultado NVARCHAR(30) NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_IntegrationLogs PRIMARY KEY CLUSTERED (IntegrationLogId)
);

CREATE TABLE NotificationLogs (
    NotificationLogId BIGINT IDENTITY(1,1) NOT NULL,
    Fecha DATETIME2(7) NOT NULL CONSTRAINT DF_NotificationLogs_Fecha DEFAULT GETUTCDATE(),
    Destinatario NVARCHAR(150) NOT NULL,
    TipoCanal NVARCHAR(20) NOT NULL,
    Asunto NVARCHAR(200) NULL,
    EstadoEnvio NVARCHAR(30) NOT NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_NotificationLogs PRIMARY KEY CLUSTERED (NotificationLogId)
);

-- 12. Dominio: Transiciones de Máquinas de Estado
CREATE TABLE StateTransitions (
    StateTransitionId INT IDENTITY(1,1) NOT NULL,
    Entidad NVARCHAR(50) NOT NULL,
    EstadoOrigenId INT NOT NULL,
    EstadoDestinoId INT NOT NULL,
    RolRequeridoId INT NULL,
    RequiereAprobacion BIT NOT NULL CONSTRAINT DF_StateTransitions_ReqAprob DEFAULT 0,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_StateTransitions_CreatedDate DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StateTransitions PRIMARY KEY CLUSTERED (StateTransitionId)
);

-- ==========================================
-- LEDGER TABLES (APPEND-ONLY)
-- Se almacenan en el Filegroup FG_SIR_Audit
-- ==========================================

CREATE TABLE AuditLogs (
    AuditId BIGINT IDENTITY(1,1) NOT NULL,
    FechaHoraUTC DATETIME2(7) NOT NULL CONSTRAINT DF_AuditLogs_Fecha DEFAULT GETUTCDATE(),
    UsuarioId INT NULL,
    UsuarioNombre NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
    Modulo NVARCHAR(50) NOT NULL,
    Entidad NVARCHAR(100) NOT NULL,
    EntidadId INT NOT NULL,
    Accion NVARCHAR(50) NOT NULL,
    EstadoAnterior NVARCHAR(MAX) NULL,
    EstadoNuevo NVARCHAR(MAX) NULL,
    Canal NVARCHAR(50) NOT NULL,
    CorrelationId UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (AuditId) -- Added explict PK to satisfy Msg 1776
)
ON FG_SIR_Audit
WITH (LEDGER = ON (APPEND_ONLY = ON));

CREATE TABLE StateHistory (
    StateHistoryId BIGINT IDENTITY(1,1) NOT NULL,
    Entidad NVARCHAR(100) NOT NULL,
    EntidadId INT NOT NULL,
    EstadoAnteriorId INT NULL,
    EstadoNuevoId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Fecha DATETIME2(7) NOT NULL CONSTRAINT DF_StateHistory_Fecha DEFAULT GETUTCDATE(),
    Comentario NVARCHAR(500) NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_StateHistory PRIMARY KEY CLUSTERED (StateHistoryId) -- Added explicit PK
)
ON FG_SIR_Audit
WITH (LEDGER = ON (APPEND_ONLY = ON));

CREATE TABLE AgentExecutions (
    ExecutionId BIGINT IDENTITY(1,1) NOT NULL,
    AgenteId INT NOT NULL,
    PromptVersionId INT NOT NULL,
    UsuarioId INT NOT NULL,
    FechaInicio DATETIME2(7) NOT NULL,
    FechaFin DATETIME2(7) NOT NULL,
    DuracionMs INT NOT NULL,
    InputJson NVARCHAR(MAX) NOT NULL,
    OutputJson NVARCHAR(MAX) NOT NULL,
    ResultadoStatus NVARCHAR(30) NOT NULL,
    TokensInput INT NOT NULL,
    TokensOutput INT NOT NULL,
    CostoEstimado DECIMAL(10,5) NOT NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AgentExecutions PRIMARY KEY CLUSTERED (ExecutionId) -- Added explicit PK to back foreign keys
)
ON FG_SIR_Audit
WITH (LEDGER = ON (APPEND_ONLY = ON));

CREATE TABLE WorkflowExecutions (
    WorkflowExecutionId BIGINT IDENTITY(1,1) NOT NULL,
    WorkflowName NVARCHAR(100) NOT NULL,
    FechaInicio DATETIME2(7) NOT NULL,
    FechaFin DATETIME2(7) NULL,
    EstadoExecution NVARCHAR(30) NOT NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_WorkflowExecutions PRIMARY KEY CLUSTERED (WorkflowExecutionId)
);

CREATE TABLE SLAExecutions (
    SLAExecutionId BIGINT IDENTITY(1,1) NOT NULL,
    SLAId INT NOT NULL,
    Entidad NVARCHAR(100) NOT NULL,
    EntidadId INT NOT NULL,
    EstadoId INT NOT NULL,
    FechaInicio DATETIME2(7) NOT NULL,
    FechaLimite DATETIME2(7) NOT NULL,
    FechaFin DATETIME2(7) NULL,
    Cumplido BIT NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_SLAExecutions_IsDeleted DEFAULT 0,
    CONSTRAINT PK_SLAExecutions PRIMARY KEY CLUSTERED (SLAExecutionId)
);

CREATE TABLE MetricSnapshot (
    MetricSnapshotId BIGINT IDENTITY(1,1) NOT NULL,
    MetricaNombre NVARCHAR(100) NOT NULL,
    FechaHora DATETIME2(7) NOT NULL CONSTRAINT DF_MetricSnapshot_Fecha DEFAULT GETUTCDATE(),
    Valor DECIMAL(18,4) NOT NULL,
    AgrupacionClave NVARCHAR(100) NULL,
    AgrupacionValor NVARCHAR(250) NULL,
    CONSTRAINT PK_MetricSnapshot PRIMARY KEY CLUSTERED (MetricSnapshotId)
);
GO
