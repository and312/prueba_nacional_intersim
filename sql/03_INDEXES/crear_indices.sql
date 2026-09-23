-- ==========================================
-- 03_INDEXES/crear_indices.sql
-- Creación de Índices y Coberturas (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Índices de Cobertura para Login y Sesión
CREATE NONCLUSTERED INDEX IX_Usuarios_Active_Cover
    ON Usuarios (Correo)
    INCLUDE (Nombre, Estado, TipoAutenticacion)
    WHERE IsDeleted = 0
    ON [FG_SIR_Indexes];
GO

-- 2. Índices Filtrados para el Directorio Corporativo de Identidad
CREATE NONCLUSTERED INDEX FIX_Usuarios_AD
    ON Usuarios (ActiveDirectoryId)
    WHERE ActiveDirectoryId IS NOT NULL AND IsDeleted = 0
    ON [FG_SIR_Indexes];
GO

-- 3. Índices para el Pipeline Kanban y Postulaciones (Corregido)
CREATE NONCLUSTERED INDEX IX_Postulaciones_FiltroPipeline
    ON Postulaciones (IsDeleted, EstadoPipelineId)
    INCLUDE (PostulanteId, VacanteId)
    ON [FG_SIR_Indexes];
GO

-- 4. Índices para Solicitudes Activas
CREATE NONCLUSTERED INDEX FIX_Solicitudes_Activas
    ON Solicitudes (IsDeleted, EstadoId)
    INCLUDE (Cargo, Area, SolicitanteId, Prioridad, FechaIdeal)
    ON [FG_SIR_Indexes];
GO

-- 5. Índices para SLA y Auditoría de Estados
CREATE NONCLUSTERED INDEX IX_SLAExecutions_Entidad_Id
    ON SLAExecutions (Entidad, EntidadId)
    INCLUDE (EstadoId, FechaInicio, FechaLimite, FechaFin, Cumplido)
    WHERE IsDeleted = 0
    ON [FG_SIR_Indexes];
GO

CREATE NONCLUSTERED INDEX IX_StateHistory_Entidad_Id
    ON StateHistory (Entidad, EntidadId)
    INCLUDE (EstadoNuevoId, Fecha, UsuarioId)
    ON [FG_SIR_Indexes];
GO

-- 6. Índices para Auditorías por CorrelationId
CREATE NONCLUSTERED INDEX IX_AuditLogs_CorrelationId
    ON AuditLogs (CorrelationId)
    WHERE CorrelationId IS NOT NULL
    ON [FG_SIR_Indexes];
GO

-- ==========================================
-- 7. ÍNDICES NO AGRUPADOS PARA CLAVES FORÁNEAS (EVITAR TABLE SCANS)
-- ==========================================
CREATE NONCLUSTERED INDEX IX_Sesiones_UsuarioId ON Sesiones (UsuarioId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Parametros_CatalogoId ON Parametros (CatalogoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Solicitudes_SolicitanteId ON Solicitudes (SolicitanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Solicitudes_DecisorId ON Solicitudes (DecisorId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Solicitudes_EstadoId ON Solicitudes (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_PerfilesCargo_SolicitudId ON PerfilesCargo (SolicitudId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_PerfilesCargo_EstadoId ON PerfilesCargo (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Vacantes_PerfilCargoId ON Vacantes (PerfilCargoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Vacantes_SolicitudId ON Vacantes (SolicitudId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Vacantes_EstadoId ON Vacantes (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Postulaciones_PostulanteId ON Postulaciones (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Postulaciones_VacanteId ON Postulaciones (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Postulaciones_EstadoPipelineId ON Postulaciones (EstadoPipelineId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Matchings_PostulanteId ON Matchings (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Matchings_VacanteId ON Matchings (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Matchings_ExecutionId ON Matchings (ExecutionId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Scorings_PostulanteId ON Scorings (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Scorings_VacanteId ON Scorings (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Scorings_ExecutionId ON Scorings (ExecutionId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Entrevistas_VacanteId ON Entrevistas (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Entrevistas_PostulanteId ON Entrevistas (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Entrevistas_EstadoId ON Entrevistas (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Ofertas_PostulanteId ON Ofertas (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Ofertas_VacanteId ON Ofertas (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Ofertas_EstadoId ON Ofertas (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Contrataciones_PostulanteId ON Contrataciones (PostulanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_Contrataciones_VacanteId ON Contrataciones (VacanteId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_SLAExecutions_SLAId ON SLAExecutions (SLAId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_SLAExecutions_EstadoId ON SLAExecutions (EstadoId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_SLAAlerts_SLAExecutionId ON SLAAlerts (SLAExecutionId) ON [FG_SIR_Indexes];
CREATE NONCLUSTERED INDEX IX_SLAEscalations_SLAExecutionId ON SLAEscalations (SLAExecutionId) ON [FG_SIR_Indexes];
GO

-- ==========================================
-- 8. CONFIGURACIÓN DEFENSIVA DE FULL TEXT SEARCH
-- ==========================================
IF SERVERPROPERTY('IsFullTextInstalled') = 1
BEGIN
    PRINT 'Iniciando creación de catálogo e índices Full-Text...';
    
    -- Crear catálogo de Full Text
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FTC_SIR')
    BEGIN
        CREATE FULLTEXT CATALOG FTC_SIR AS DEFAULT;
    END

    -- Crear índice en PerfilesCargo
    IF NOT EXISTS (
        SELECT 1 FROM sys.fulltext_indexes fi 
        JOIN sys.objects o ON fi.object_id = o.object_id 
        WHERE o.name = 'PerfilesCargo'
    )
    BEGIN
        CREATE FULLTEXT INDEX ON PerfilesCargo(Descripcion)
        KEY INDEX PK_PerfilesCargo
        WITH STOPLIST = SYSTEM;
    END

    -- Crear índice en Solicitudes
    IF NOT EXISTS (
        SELECT 1 FROM sys.fulltext_indexes fi 
        JOIN sys.objects o ON fi.object_id = o.object_id 
        WHERE o.name = 'Solicitudes'
    )
    BEGIN
        CREATE FULLTEXT INDEX ON Solicitudes(Funciones, Skills)
        KEY INDEX PK_Solicitudes
        WITH STOPLIST = SYSTEM;
    END
    
    PRINT 'Catálogo e índices Full-Text creados exitosamente.';
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: El servicio Full-Text Search no está instalado o activo. Se omitió la creación del catálogo e índices de texto completo.';
END
GO
