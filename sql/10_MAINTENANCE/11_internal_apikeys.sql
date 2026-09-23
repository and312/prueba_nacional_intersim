-- ==========================================
-- SCRIPT DE MIGRACIÓN: AUTENTICACIÓN HÍBRIDA POR API KEYS
-- ==========================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Crear Tabla ApiKeys
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeys')
BEGIN
    CREATE TABLE ApiKeys (
        ApiKeyId BIGINT IDENTITY(1,1) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        ApiKeyHash NVARCHAR(256) NOT NULL,
        Workflow NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(250) NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_ApiKeys_Estado DEFAULT 'Activo',
        FechaCreacion DATETIME2 NOT NULL,
        FechaExpiracion DATETIME2 NULL,
        UltimoUso DATETIME2 NULL,
        UltimaIP NVARCHAR(45) NULL,
        CreadoPor NVARCHAR(100) NOT NULL,
        Permisos NVARCHAR(1000) NOT NULL,
        CONSTRAINT PK_ApiKeys PRIMARY KEY CLUSTERED (ApiKeyId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UQ_ApiKeys_ApiKeyHash ON ApiKeys(ApiKeyHash);
    PRINT 'Tabla ApiKeys creada con éxito.';
END
ELSE
BEGIN
    PRINT 'La tabla ApiKeys ya existe.';
END
GO

-- 2. Modificar Tabla Solicitudes para agregar campos de trazabilidad
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Solicitudes')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Solicitudes') AND name = 'CanalOrigen')
    BEGIN
        ALTER TABLE Solicitudes
        ADD CanalOrigen NVARCHAR(50) NOT NULL CONSTRAINT DF_Solicitudes_CanalOrigen DEFAULT 'BackOffice';
        PRINT 'Columna CanalOrigen agregada a Solicitudes.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Solicitudes') AND name = 'WorkflowOrigen')
    BEGIN
        ALTER TABLE Solicitudes
        ADD WorkflowOrigen NVARCHAR(100) NULL;
        PRINT 'Columna WorkflowOrigen agregada a Solicitudes.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Solicitudes') AND name = 'ApiKeyId')
    BEGIN
        ALTER TABLE Solicitudes
        ADD ApiKeyId BIGINT NULL;
        PRINT 'Columna ApiKeyId agregada a Solicitudes.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Solicitudes') AND name = 'CorrelationId')
    BEGIN
        ALTER TABLE Solicitudes
        ADD CorrelationId UNIQUEIDENTIFIER NULL;
        PRINT 'Columna CorrelationId agregada a Solicitudes.';
    END

    -- Agregar la llave foránea
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Solicitudes_ApiKeys')
    BEGIN
        ALTER TABLE Solicitudes
        ADD CONSTRAINT FK_Solicitudes_ApiKeys FOREIGN KEY (ApiKeyId) REFERENCES ApiKeys (ApiKeyId);
        PRINT 'Restricción FK_Solicitudes_ApiKeys agregada.';
    END
END
GO
