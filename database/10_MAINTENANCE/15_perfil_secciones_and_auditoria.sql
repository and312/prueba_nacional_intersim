-- ========================================================
-- 10_MAINTENANCE/15_perfil_secciones_and_auditoria.sql
-- Cambios para la reestructuración y versionado por secciones del módulo Perfiles
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR)
-- ========================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE SIR_NacionalSeguros;
GO

PRINT 'Aplicando cambios en tabla PerfilesCargo...';

-- 1. Agregar columnas a PerfilesCargo si no existen
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PerfilesCargo') AND name = 'PdfUrl')
BEGIN
    ALTER TABLE dbo.PerfilesCargo ADD PdfUrl NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PerfilesCargo') AND name = 'JsonOriginalIA')
BEGIN
    ALTER TABLE dbo.PerfilesCargo ADD JsonOriginalIA NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PerfilesCargo') AND name = 'JsonActual')
BEGIN
    ALTER TABLE dbo.PerfilesCargo ADD JsonActual NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PerfilesCargo') AND name = 'Activo')
BEGIN
    ALTER TABLE dbo.PerfilesCargo ADD Activo BIT NOT NULL DEFAULT 1;
END
GO

-- 2. Crear tabla PerfilSecciones si no existe
PRINT 'Creando tabla PerfilSecciones...';
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('dbo.PerfilSecciones'))
BEGIN
    CREATE TABLE dbo.PerfilSecciones (
        PerfilSeccionId INT IDENTITY(1,1) NOT NULL,
        PerfilCargoId INT NOT NULL,
        NumeroSeccion INT NOT NULL,
        NombreSeccion NVARCHAR(200) NOT NULL,
        Contenido NVARCHAR(MAX) NOT NULL,
        Orden INT NOT NULL,
        UltimaActualizacion DATETIME2(7) NOT NULL CONSTRAINT DF_PerfilSecciones_UltimaActualizacion DEFAULT SYSUTCDATETIME(),
        UsuarioActualizacion NVARCHAR(100) NOT NULL,
        CONSTRAINT PK_PerfilSecciones PRIMARY KEY CLUSTERED (PerfilSeccionId),
        CONSTRAINT FK_PerfilSecciones_PerfilesCargo FOREIGN KEY (PerfilCargoId) REFERENCES dbo.PerfilesCargo(PerfilCargoId),
        CONSTRAINT UQ_PerfilSecciones_PerfilCargo_Numero UNIQUE (PerfilCargoId, NumeroSeccion)
    );
END
GO

-- 3. Crear tabla PerfilAuditoria si no existe
PRINT 'Creando tabla PerfilAuditoria...';
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('dbo.PerfilAuditoria'))
BEGIN
    CREATE TABLE dbo.PerfilAuditoria (
        PerfilAuditoriaId INT IDENTITY(1,1) NOT NULL,
        PerfilCargoId INT NOT NULL,
        PerfilSeccionId INT NULL,
        Version INT NOT NULL,
        SeccionModificada NVARCHAR(200) NULL,
        ValorAnterior NVARCHAR(MAX) NULL,
        ValorNuevo NVARCHAR(MAX) NULL,
        Usuario NVARCHAR(100) NOT NULL,
        FechaHora DATETIME2(7) NOT NULL CONSTRAINT DF_PerfilAuditoria_FechaHora DEFAULT SYSUTCDATETIME(),
        MotivoCambio NVARCHAR(1000) NULL,
        EstadoPerfil NVARCHAR(50) NULL,
        CONSTRAINT PK_PerfilAuditoria PRIMARY KEY CLUSTERED (PerfilAuditoriaId),
        CONSTRAINT FK_PerfilAuditoria_PerfilesCargo FOREIGN KEY (PerfilCargoId) REFERENCES dbo.PerfilesCargo(PerfilCargoId),
        CONSTRAINT FK_PerfilAuditoria_PerfilSecciones FOREIGN KEY (PerfilSeccionId) REFERENCES dbo.PerfilSecciones(PerfilSeccionId)
    );
END
GO

PRINT 'Migración por secciones de Perfiles creada exitosamente.';
GO
