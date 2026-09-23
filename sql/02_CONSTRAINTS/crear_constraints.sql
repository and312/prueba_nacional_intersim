-- ==========================================
-- 02_CONSTRAINTS/crear_constraints.sql
-- Definición de Restricciones (FK, CK, UQ) (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. RESTRICCIONES DE UNICIDAD (UNIQUE CONSTRAINTS)
-- ==========================================

ALTER TABLE Usuarios ADD CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo);
ALTER TABLE Roles ADD CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre);
ALTER TABLE Permisos ADD CONSTRAINT UQ_Permisos_Codigo UNIQUE (Codigo);
ALTER TABLE Sesiones ADD CONSTRAINT UQ_Sesiones_RefreshToken UNIQUE (RefreshToken);
ALTER TABLE SLAs ADD CONSTRAINT UQ_SLAs_Nombre UNIQUE (Nombre);
ALTER TABLE Catalogos ADD CONSTRAINT UQ_Catalogos_Codigo UNIQUE (Codigo);
ALTER TABLE Parametros ADD CONSTRAINT UQ_Parametros_Catalogo_Codigo UNIQUE (CatalogoId, Codigo);
ALTER TABLE Estados ADD CONSTRAINT UQ_Estados_Codigo UNIQUE (Codigo);
ALTER TABLE Feriados ADD CONSTRAINT UQ_Feriados_Fecha UNIQUE (Fecha);
ALTER TABLE PerfilesCargo ADD CONSTRAINT UQ_PerfilesCargo_Solicitud_Version UNIQUE (SolicitudId, Version);
ALTER TABLE Postulantes ADD CONSTRAINT UQ_Postulantes_Documento UNIQUE (DocumentoIdentidad);
ALTER TABLE Matchings ADD CONSTRAINT UQ_Matchings_Postulante_Vacante UNIQUE (PostulanteId, VacanteId);
ALTER TABLE Scorings ADD CONSTRAINT UQ_Scorings_Postulante_Vacante UNIQUE (PostulanteId, VacanteId);
ALTER TABLE PromptVersions ADD CONSTRAINT UQ_PromptVersions_Prompt_Version UNIQUE (PromptId, VersionNumber);
ALTER TABLE Prompts ADD CONSTRAINT UQ_Prompts_Nombre UNIQUE (Nombre);
ALTER TABLE Agentes ADD CONSTRAINT UQ_Agentes_Nombre UNIQUE (Nombre);
ALTER TABLE Postulaciones ADD CONSTRAINT UQ_Postulaciones_Postulante_Vacante UNIQUE (PostulanteId, VacanteId);
GO

-- ==========================================
-- 2. RESTRICCIONES DE VALIDACIÓN (CHECK CONSTRAINTS)
-- ==========================================

-- Usuarios
ALTER TABLE Usuarios ADD CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN ('Activo', 'Inactivo'));
ALTER TABLE Usuarios ADD CONSTRAINT CK_Usuarios_TipoAutenticacion CHECK (TipoAutenticacion IN ('Local', 'ActiveDirectory'));
ALTER TABLE Usuarios ADD CONSTRAINT CK_Usuarios_ClaveHash_AD CHECK (
    (TipoAutenticacion = 'Local' AND ClaveHash IS NOT NULL) OR 
    (TipoAutenticacion = 'ActiveDirectory' AND ClaveHash IS NULL)
);

-- Solicitudes
ALTER TABLE Solicitudes ADD CONSTRAINT CK_Solicitudes_Modalidad CHECK (Modalidad IN ('Presencial', 'Teletrabajo', 'Hibrido'));
ALTER TABLE Solicitudes ADD CONSTRAINT CK_Solicitudes_Seniority CHECK (Seniority IN ('Junior', 'SemiSenior', 'Senior'));
ALTER TABLE Solicitudes ADD CONSTRAINT CK_Solicitudes_Prioridad CHECK (Prioridad IN ('Baja', 'Media', 'Alta', 'Critica'));

-- Postulantes
ALTER TABLE Postulantes ADD CONSTRAINT CK_Postulantes_Origen CHECK (Origen IN ('LinkedIn', 'Web', 'Referido', 'Manual'));

-- Entrevistas
ALTER TABLE Entrevistas ADD CONSTRAINT CK_Entrevistas_TipoEntrevista CHECK (TipoEntrevista IN ('Presencial', 'Teams', 'Llamada'));
ALTER TABLE Entrevistas ADD CONSTRAINT CK_Entrevistas_Reprogramaciones CHECK (ReprogramacionesContador <= 3);

-- SLAs
ALTER TABLE SLAs ADD CONSTRAINT CK_SLAs_DiasMaximos CHECK (DiasMaximos > 0);

-- Estados
ALTER TABLE Estados ADD CONSTRAINT CK_Estados_Entidad CHECK (Entidad IN ('Solicitud', 'Vacante', 'Postulante', 'Entrevista', 'Oferta'));

-- SLAExecutions
ALTER TABLE SLAExecutions ADD CONSTRAINT CK_SLAExecutions_Entidad CHECK (Entidad IN ('Solicitud', 'Vacante', 'Postulante'));
GO

-- ==========================================
-- 3. CLAVES FORÁNEAS (FOREIGN KEYS)
-- ==========================================

-- RolPermisos
ALTER TABLE RolPermisos ADD CONSTRAINT FK_RolPermisos_Roles 
    FOREIGN KEY (RolId) REFERENCES Roles(RolId);
ALTER TABLE RolPermisos ADD CONSTRAINT FK_RolPermisos_Permisos 
    FOREIGN KEY (PermisoId) REFERENCES Permisos(PermisoId);

-- UsuarioRoles
ALTER TABLE UsuarioRoles ADD CONSTRAINT FK_UsuarioRoles_Usuarios 
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId);
ALTER TABLE UsuarioRoles ADD CONSTRAINT FK_UsuarioRoles_Roles 
    FOREIGN KEY (RolId) REFERENCES Roles(RolId);

-- Sesiones
ALTER TABLE Sesiones ADD CONSTRAINT FK_Sesiones_Usuarios 
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId);

-- Parametros
ALTER TABLE Parametros ADD CONSTRAINT FK_Parametros_Catalogos 
    FOREIGN KEY (CatalogoId) REFERENCES Catalogos(CatalogoId);
ALTER TABLE Parametros ADD CONSTRAINT FK_Parametros_Parametros_Padre 
    FOREIGN KEY (ParametroIdPadre) REFERENCES Parametros(ParametroId);

-- Estados
ALTER TABLE Estados ADD CONSTRAINT FK_Estados_SLAs 
    FOREIGN KEY (SLAId) REFERENCES SLAs(SLAId);

-- Solicitudes
ALTER TABLE Solicitudes ADD CONSTRAINT FK_Solicitudes_Usuarios_Solicitante 
    FOREIGN KEY (SolicitanteId) REFERENCES Usuarios(UsuarioId);
ALTER TABLE Solicitudes ADD CONSTRAINT FK_Solicitudes_Usuarios_Decisor 
    FOREIGN KEY (DecisorId) REFERENCES Usuarios(UsuarioId);
ALTER TABLE Solicitudes ADD CONSTRAINT FK_Solicitudes_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);

-- SolicitudComentarios
ALTER TABLE SolicitudComentarios ADD CONSTRAINT FK_SolicitudComentarios_Solicitudes 
    FOREIGN KEY (SolicitudId) REFERENCES Solicitudes(SolicitudId);
ALTER TABLE SolicitudComentarios ADD CONSTRAINT FK_SolicitudComentarios_Usuarios 
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId);

-- PerfilesCargo
ALTER TABLE PerfilesCargo ADD CONSTRAINT FK_PerfilesCargo_Solicitudes 
    FOREIGN KEY (SolicitudId) REFERENCES Solicitudes(SolicitudId);
ALTER TABLE PerfilesCargo ADD CONSTRAINT FK_PerfilesCargo_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);

-- Vacantes
ALTER TABLE Vacantes ADD CONSTRAINT FK_Vacantes_PerfilesCargo 
    FOREIGN KEY (PerfilCargoId) REFERENCES PerfilesCargo(PerfilCargoId);
ALTER TABLE Vacantes ADD CONSTRAINT FK_Vacantes_Solicitudes 
    FOREIGN KEY (SolicitudId) REFERENCES Solicitudes(SolicitudId);
ALTER TABLE Vacantes ADD CONSTRAINT FK_Vacantes_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);

-- Postulaciones (Fila normalizada de enlace M:N)
ALTER TABLE Postulaciones ADD CONSTRAINT FK_Postulaciones_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Postulaciones ADD CONSTRAINT FK_Postulaciones_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);
ALTER TABLE Postulaciones ADD CONSTRAINT FK_Postulaciones_Estados 
    FOREIGN KEY (EstadoPipelineId) REFERENCES Estados(EstadoId);

-- Matchings
ALTER TABLE Matchings ADD CONSTRAINT FK_Matchings_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Matchings ADD CONSTRAINT FK_Matchings_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);
ALTER TABLE Matchings ADD CONSTRAINT FK_Matchings_AgentExecutions 
    FOREIGN KEY (ExecutionId) REFERENCES AgentExecutions(ExecutionId);

-- Scorings
ALTER TABLE Scorings ADD CONSTRAINT FK_Scorings_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Scorings ADD CONSTRAINT FK_Scorings_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);
ALTER TABLE Scorings ADD CONSTRAINT FK_Scorings_AgentExecutions 
    FOREIGN KEY (ExecutionId) REFERENCES AgentExecutions(ExecutionId);

-- Entrevistas
ALTER TABLE Entrevistas ADD CONSTRAINT FK_Entrevistas_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);
ALTER TABLE Entrevistas ADD CONSTRAINT FK_Entrevistas_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Entrevistas ADD CONSTRAINT FK_Entrevistas_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);

-- Ofertas
ALTER TABLE Ofertas ADD CONSTRAINT FK_Ofertas_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Ofertas ADD CONSTRAINT FK_Ofertas_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);
ALTER TABLE Ofertas ADD CONSTRAINT FK_Ofertas_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);

-- Contrataciones
ALTER TABLE Contrataciones ADD CONSTRAINT FK_Contrataciones_Postulantes 
    FOREIGN KEY (PostulanteId) REFERENCES Postulantes(PostulanteId);
ALTER TABLE Contrataciones ADD CONSTRAINT FK_Contrataciones_Vacantes 
    FOREIGN KEY (VacanteId) REFERENCES Vacantes(VacanteId);

-- PromptVersions
ALTER TABLE PromptVersions ADD CONSTRAINT FK_PromptVersions_Prompts 
    FOREIGN KEY (PromptId) REFERENCES Prompts(PromptId);

-- ModelExecutions
ALTER TABLE ModelExecutions ADD CONSTRAINT FK_ModelExecutions_AgentExecutions 
    FOREIGN KEY (ExecutionId) REFERENCES AgentExecutions(ExecutionId);

-- TokenConsumptions
ALTER TABLE TokenConsumptions ADD CONSTRAINT FK_TokenConsumptions_AgentExecutions 
    FOREIGN KEY (ExecutionId) REFERENCES AgentExecutions(ExecutionId);

-- SLAAlerts
ALTER TABLE SLAAlerts ADD CONSTRAINT FK_SLAAlerts_SLAExecutions 
    FOREIGN KEY (SLAExecutionId) REFERENCES SLAExecutions(SLAExecutionId);

-- SLAEscalations
ALTER TABLE SLAEscalations ADD CONSTRAINT FK_SLAEscalations_SLAExecutions 
    FOREIGN KEY (SLAExecutionId) REFERENCES SLAExecutions(SLAExecutionId);
ALTER TABLE SLAEscalations ADD CONSTRAINT FK_SLAEscalations_Usuarios_Original 
    FOREIGN KEY (ResponsableOriginalId) REFERENCES Usuarios(UsuarioId);
ALTER TABLE SLAEscalations ADD CONSTRAINT FK_SLAEscalations_Usuarios_Nuevo 
    FOREIGN KEY (ResponsableNuevoId) REFERENCES Usuarios(UsuarioId);

-- StateTransitions
ALTER TABLE StateTransitions ADD CONSTRAINT FK_StateTransitions_Estados_Origen 
    FOREIGN KEY (EstadoOrigenId) REFERENCES Estados(EstadoId);
ALTER TABLE StateTransitions ADD CONSTRAINT FK_StateTransitions_Estados_Destino 
    FOREIGN KEY (EstadoDestinoId) REFERENCES Estados(EstadoId);
ALTER TABLE StateTransitions ADD CONSTRAINT FK_StateTransitions_Roles 
    FOREIGN KEY (RolRequeridoId) REFERENCES Roles(RolId);

-- SLAExecutions
ALTER TABLE SLAExecutions ADD CONSTRAINT FK_SLAExecutions_SLAs 
    FOREIGN KEY (SLAId) REFERENCES SLAs(SLAId);
ALTER TABLE SLAExecutions ADD CONSTRAINT FK_SLAExecutions_Estados 
    FOREIGN KEY (EstadoId) REFERENCES Estados(EstadoId);
GO
