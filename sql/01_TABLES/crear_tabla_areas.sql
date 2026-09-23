-- ==========================================
-- sql/01_TABLES/crear_tabla_areas.sql
-- Creación de la tabla Areas y adición de campos de Módulo 01 en Usuarios
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- ==========================================

USE SIR_NacionalSeguros;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Crear tabla Areas si no existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Areas]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Areas (
        AreaId INT IDENTITY(1,1) NOT NULL,
        Codigo NVARCHAR(20) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Gerencia NVARCHAR(100) NOT NULL,
        Responsable NVARCHAR(100) NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Areas_Estado DEFAULT 'Activo',
        CreatedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_Areas_CreatedBy DEFAULT 'SYSTEM',
        CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Areas_CreatedDate DEFAULT GETUTCDATE(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2(7) NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Areas_IsDeleted DEFAULT 0,
        CONSTRAINT PK_Areas PRIMARY KEY CLUSTERED (AreaId),
        CONSTRAINT UQ_Areas_Codigo UNIQUE (Codigo),
        CONSTRAINT CK_Areas_Estado CHECK (Estado IN ('Activo', 'Inactivo'))
    );
END
GO

-- 2. Agregar campos a la tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'AreaId')
BEGIN
    ALTER TABLE dbo.Usuarios ADD AreaId INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Nombres')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Nombres NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Apellidos')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Apellidos NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Cargo')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Cargo NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Gerencia')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Gerencia NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Telefono')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Telefono NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Extension')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Extension NVARCHAR(10) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'Observaciones')
BEGIN
    ALTER TABLE dbo.Usuarios ADD Observaciones NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'FotografiaUrl')
BEGIN
    ALTER TABLE dbo.Usuarios ADD FotografiaUrl NVARCHAR(250) NULL;
END
GO

-- 3. Insertar Áreas por defecto si no existen
IF NOT EXISTS (SELECT * FROM dbo.Areas WHERE Codigo = 'ARR-TI')
BEGIN
    INSERT INTO dbo.Areas (Codigo, Nombre, Gerencia, Responsable, Estado)
    VALUES (N'ARR-TI', N'Tecnología de Información', N'Gerencia de Tecnología', N'admin@nacionalseguros.com.bo', N'Activo');
END

IF NOT EXISTS (SELECT * FROM dbo.Areas WHERE Codigo = 'ARR-RRHH')
BEGIN
    INSERT INTO dbo.Areas (Codigo, Nombre, Gerencia, Responsable, Estado)
    VALUES (N'ARR-RRHH', N'Recursos Humanos', N'Gerencia de Capital Humano', N'rrhh@nacionalseguros.com.bo', N'Activo');
END

IF NOT EXISTS (SELECT * FROM dbo.Areas WHERE Codigo = 'ARR-FIN')
BEGIN
    INSERT INTO dbo.Areas (Codigo, Nombre, Gerencia, Responsable, Estado)
    VALUES (N'ARR-FIN', N'Finanzas', N'Gerencia de Finanzas', N'finanzas@nacionalseguros.com.bo', N'Activo');
END
GO

-- 4. Asignar área por defecto a los usuarios existentes
DECLARE @TiAreaId INT = (SELECT AreaId FROM dbo.Areas WHERE Codigo = 'ARR-TI');
DECLARE @RrhhAreaId INT = (SELECT AreaId FROM dbo.Areas WHERE Codigo = 'ARR-RRHH');
DECLARE @FinAreaId INT = (SELECT AreaId FROM dbo.Areas WHERE Codigo = 'ARR-FIN');

UPDATE dbo.Usuarios 
SET AreaId = @RrhhAreaId,
    Nombres = N'Recursos',
    Apellidos = N'Humanos',
    Cargo = N'Jefe de Reclutamiento',
    Gerencia = N'Capital Humano'
WHERE Correo LIKE 'rrhh%' AND AreaId IS NULL;

UPDATE dbo.Usuarios 
SET AreaId = @FinAreaId,
    Nombres = N'Finanzas',
    Apellidos = N'Corporativas',
    Cargo = N'Jefe de Finanzas',
    Gerencia = N'Finanzas y Control'
WHERE Correo LIKE 'finanzas%' AND AreaId IS NULL;

UPDATE dbo.Usuarios 
SET AreaId = @TiAreaId,
    Nombres = N'Administrador',
    Apellidos = N'TI',
    Cargo = N'Administrador de Sistemas',
    Gerencia = N'Tecnología y Procesos'
WHERE AreaId IS NULL;
GO

-- 5. Hacer AreaId NOT NULL y agregar clave foránea
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'AreaId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Usuarios ALTER COLUMN AreaId INT NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Usuarios_Areas')
BEGIN
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT FK_Usuarios_Areas FOREIGN KEY (AreaId) REFERENCES dbo.Areas (AreaId);
END
GO
