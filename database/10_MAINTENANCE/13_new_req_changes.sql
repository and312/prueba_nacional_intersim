-- ========================================================
-- 10_MAINTENANCE/13_new_req_changes.sql
-- Cambios para las Historias de Usuario HU3 y HU4 (Resumidor y PDF)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR)
-- ========================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE SIR_NacionalSeguros;
GO

PRINT 'Aplicando cambios en tabla Solicitudes...';

-- 1. Agregar columnas requeridas y opcionales a Solicitudes
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'RemuneracionOfrecida')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD RemuneracionOfrecida DECIMAL(18,2) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'TipoSolicitud')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD TipoSolicitud NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'Motivo')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD Motivo NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'CantidadVacantes')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD CantidadVacantes INT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'Ubicacion')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD Ubicacion NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'Observaciones')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD Observaciones NVARCHAR(MAX) NULL;
END

-- 2. Agregar columna calculada persistida para Código de solicitud (ej: SOL-2026-0001)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Solicitudes') AND name = 'Codigo')
BEGIN
    ALTER TABLE dbo.Solicitudes ADD Codigo AS (
        'SOL-' + CONVERT(nvarchar(4), DATEPART(year, CreatedDate)) + '-' + RIGHT('0000' + CONVERT(nvarchar(10), SolicitudId), 4)
    ) PERSISTED;
END
GO

-- 3. Crear tabla SolicitudResumenes
PRINT 'Creando tabla SolicitudResumenes...';
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('dbo.SolicitudResumenes'))
BEGIN
    CREATE TABLE dbo.SolicitudResumenes (
        ResumenId INT IDENTITY(1,1) NOT NULL,
        SolicitudId INT NOT NULL,
        ProfileSummary NVARCHAR(500) NULL,
        CompletitudPorcentaje DECIMAL(5,2) NULL,
        CamposDetectados INT NULL,
        CamposEsperados INT NULL,
        CaptureState NVARCHAR(30) NOT NULL,
        CaptureConfidence DECIMAL(4,2) NULL,
        Recommendation NVARCHAR(60) NOT NULL,
        CamposFaltantesJson NVARCHAR(MAX) NULL,
        InconsistenciasJson NVARCHAR(MAX) NULL,
        AgentName NVARCHAR(60) NOT NULL,
        AgentVersion NVARCHAR(20) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_SolicitudResumenes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedDate DATETIME2 NULL,
        CONSTRAINT PK_SolicitudResumenes PRIMARY KEY CLUSTERED (ResumenId),
        CONSTRAINT UQ_SolicitudResumenes_Solicitud UNIQUE (SolicitudId),
        CONSTRAINT FK_SolicitudResumenes_Solicitudes FOREIGN KEY (SolicitudId) REFERENCES dbo.Solicitudes(SolicitudId)
    );
END
GO

-- 4. Crear tabla SolicitudDocumentos
PRINT 'Creando tabla SolicitudDocumentos...';
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('dbo.SolicitudDocumentos'))
BEGIN
    CREATE TABLE dbo.SolicitudDocumentos (
        DocumentoId INT IDENTITY(1,1) NOT NULL,
        SolicitudId INT NOT NULL,
        TipoDocumento NVARCHAR(60) NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        StorageProvider NVARCHAR(40) NOT NULL,
        StoragePath NVARCHAR(500) NOT NULL,
        PublicUrl NVARCHAR(1000) NULL,
        GeneradoPor NVARCHAR(100) NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_SolicitudDocumentos_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_SolicitudDocumentos PRIMARY KEY CLUSTERED (DocumentoId),
        CONSTRAINT FK_SolicitudDocumentos_Solicitudes FOREIGN KEY (SolicitudId) REFERENCES dbo.Solicitudes(SolicitudId)
    );
END
GO

-- 5. Sembrar catálogo y parámetros para campos esperados de completitud
PRINT 'Sembrando catálogo y parámetros de completitud...';
IF NOT EXISTS (SELECT 1 FROM dbo.Catalogos WHERE Codigo = 'CAT-REQ-FIELDS')
BEGIN
    INSERT INTO dbo.Catalogos (Nombre, Codigo, CreatedBy)
    VALUES (N'Campos Requeridos de Solicitud', N'CAT-REQ-FIELDS', N'SYSTEM');

    DECLARE @CatId INT = (SELECT CatalogoId FROM dbo.Catalogos WHERE Codigo = 'CAT-REQ-FIELDS');

    INSERT INTO dbo.Parametros (CatalogoId, Codigo, Valor, CreatedBy)
    VALUES
    (@CatId, N'cargo', N'Cargo', N'SYSTEM'),
    (@CatId, N'area', N'Área', N'SYSTEM'),
    (@CatId, N'modalidad', N'Modalidad', N'SYSTEM'),
    (@CatId, N'seniority', N'Seniority', N'SYSTEM'),
    (@CatId, N'prioridad', N'Prioridad', N'SYSTEM'),
    (@CatId, N'fechaIdeal', N'Fecha Ideal', N'SYSTEM'),
    (@CatId, N'funciones', N'Funciones', N'SYSTEM'),
    (@CatId, N'skills', N'Skills/Habilidades', N'SYSTEM'),
    (@CatId, N'jornada', N'Jornada Laboral', N'SYSTEM'),
    (@CatId, N'tipoSolicitud', N'Tipo de Solicitud', N'SYSTEM'),
    (@CatId, N'motivo', N'Motivo del Requerimiento', N'SYSTEM'),
    (@CatId, N'ubicacion', N'Ubicación / Sede', N'SYSTEM'),
    (@CatId, N'remuneracionOfrecida', N'Remuneración Ofrecida', N'SYSTEM');
END
GO

PRINT 'Cambios aplicados exitosamente.';
GO
