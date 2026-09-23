-- ==========================================
-- 00_DATABASE/crear_db.sql
-- Creación de Base de Datos y Filegroups
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- Compatibility Level: 160
-- ==========================================

USE master;
GO

IF DB_ID('SIR_NacionalSeguros') IS NOT NULL
BEGIN
    ALTER DATABASE SIR_NacionalSeguros SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SIR_NacionalSeguros;
END
GO

CREATE DATABASE SIR_NacionalSeguros;
GO

USE SIR_NacionalSeguros;
GO

-- Adición de Filegroups
ALTER DATABASE SIR_NacionalSeguros ADD FILEGROUP FG_SIR_Audit;
ALTER DATABASE SIR_NacionalSeguros ADD FILEGROUP FG_SIR_Indexes;
GO

-- Adición de archivos a Filegroups de forma dinámica
DECLARE @DefaultDataPath NVARCHAR(512);
SET @DefaultDataPath = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(512));

IF @DefaultDataPath IS NULL
    SET @DefaultDataPath = N'/var/opt/mssql/data/';

DECLARE @sql NVARCHAR(MAX);
SET @sql = N'ALTER DATABASE SIR_NacionalSeguros ADD FILE (
    NAME = SIR_Audit_01, 
    FILENAME = ''' + @DefaultDataPath + N'SIR_NacionalSeguros_Audit_01.ndf'', 
    SIZE = 100MB, 
    FILEGROWTH = 10MB
) TO FILEGROUP FG_SIR_Audit;';
EXEC sp_executesql @sql;

SET @sql = N'ALTER DATABASE SIR_NacionalSeguros ADD FILE (
    NAME = SIR_Indexes_01, 
    FILENAME = ''' + @DefaultDataPath + N'SIR_NacionalSeguros_Indexes_01.ndf'', 
    SIZE = 50MB, 
    FILEGROWTH = 5MB
) TO FILEGROUP FG_SIR_Indexes;';
EXEC sp_executesql @sql;
GO

-- Establecer nivel de compatibilidad
ALTER DATABASE SIR_NacionalSeguros SET COMPATIBILITY_LEVEL = 160;
GO

-- Habilitar y configurar Query Store
ALTER DATABASE SIR_NacionalSeguros SET QUERY_STORE = ON;
GO

ALTER DATABASE SIR_NacionalSeguros SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO,
    MAX_PLANS_PER_QUERY = 200
);
GO
