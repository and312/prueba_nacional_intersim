-- ==========================================
-- SCRIPT DE MIGRACIÓN: MÓDULO 15 - GESTIÓN DE INTEGRACIONES Y API KEYS
-- ==========================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Crear Tabla Integraciones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Integraciones')
BEGIN
    CREATE TABLE Integraciones (
        IntegracionId INT IDENTITY(1,1) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Codigo NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Tipo NVARCHAR(50) NOT NULL,
        Responsable NVARCHAR(100) NOT NULL,
        CorreoResponsable NVARCHAR(100) NOT NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Integraciones_Estado DEFAULT 'Activo',
        Observaciones NVARCHAR(1000) NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        DeletedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Integraciones_IsDeleted DEFAULT 0,
        CONSTRAINT PK_Integraciones PRIMARY KEY CLUSTERED (IntegracionId),
        CONSTRAINT UQ_Integraciones_Codigo UNIQUE (Codigo)
    );
    PRINT 'Tabla Integraciones creada con éxito.';
END
ELSE
BEGIN
    PRINT 'La tabla Integraciones ya existe.';
END
GO

-- 2. Modificar Tabla ApiKeys
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeys')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'IntegracionId')
    BEGIN
        ALTER TABLE ApiKeys ADD IntegracionId INT NULL;
        PRINT 'Columna IntegracionId agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'Observaciones')
    BEGIN
        ALTER TABLE ApiKeys ADD Observaciones NVARCHAR(1000) NULL;
        PRINT 'Columna Observaciones agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'ModifiedBy')
    BEGIN
        ALTER TABLE ApiKeys ADD ModifiedBy NVARCHAR(100) NULL;
        PRINT 'Columna ModifiedBy agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'ModifiedDate')
    BEGIN
        ALTER TABLE ApiKeys ADD ModifiedDate DATETIME2 NULL;
        PRINT 'Columna ModifiedDate agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'DeletedBy')
    BEGIN
        ALTER TABLE ApiKeys ADD DeletedBy NVARCHAR(100) NULL;
        PRINT 'Columna DeletedBy agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'DeletedDate')
    BEGIN
        ALTER TABLE ApiKeys ADD DeletedDate DATETIME2 NULL;
        PRINT 'Columna DeletedDate agregada a ApiKeys.';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ApiKeys') AND name = 'IsDeleted')
    BEGIN
        ALTER TABLE ApiKeys ADD IsDeleted BIT NOT NULL CONSTRAINT DF_ApiKeys_IsDeleted DEFAULT 0;
        PRINT 'Columna IsDeleted agregada a ApiKeys.';
    END

    -- Agregar la llave foránea a Integraciones
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ApiKeys_Integraciones')
    BEGIN
        ALTER TABLE ApiKeys ADD CONSTRAINT FK_ApiKeys_Integraciones FOREIGN KEY (IntegracionId) REFERENCES Integraciones(IntegracionId);
        PRINT 'Restricción FK_ApiKeys_Integraciones agregada.';
    END
END
GO

-- 3. Crear Integración por Defecto y enlazar llaves existentes
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Integraciones') AND EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeys')
BEGIN
    DECLARE @DefaultIntegracionId INT;
    
    IF NOT EXISTS (SELECT * FROM Integraciones WHERE Codigo = 'SYS_INT')
    BEGIN
        INSERT INTO Integraciones (Nombre, Codigo, Descripcion, Tipo, Responsable, CorreoResponsable, Estado, CreatedBy, CreatedDate, IsDeleted)
        VALUES ('Sistema de Integraciones Internas', 'SYS_INT', 'Integración del sistema por defecto para flujos n8n preexistentes', 'Workflow', 'Administrador', 'admin@nacionalseguros.com.bo', 'Activo', 'System', GETUTCDATE(), 0);
        
        SET @DefaultIntegracionId = SCOPE_IDENTITY();
        PRINT 'Integración por defecto SYS_INT creada.';
    END
    ELSE
    BEGIN
        SELECT @DefaultIntegracionId = IntegracionId FROM Integraciones WHERE Codigo = 'SYS_INT';
    END

    -- Enlazar cualquier ApiKey huérfana
    UPDATE ApiKeys
    SET IntegracionId = @DefaultIntegracionId
    WHERE IntegracionId IS NULL;
    
    PRINT 'ApiKeys existentes enlazadas a la integración por defecto.';
END
GO

-- 4. Crear Tabla ApiKeyPermisos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeyPermisos')
BEGIN
    CREATE TABLE ApiKeyPermisos (
        ApiKeyId BIGINT NOT NULL,
        PermisoId INT NOT NULL,
        CONSTRAINT PK_ApiKeyPermisos PRIMARY KEY CLUSTERED (ApiKeyId, PermisoId),
        CONSTRAINT FK_ApiKeyPermisos_ApiKeys FOREIGN KEY (ApiKeyId) REFERENCES ApiKeys(ApiKeyId) ON DELETE CASCADE,
        CONSTRAINT FK_ApiKeyPermisos_Permisos FOREIGN KEY (PermisoId) REFERENCES Permisos(PermisoId) ON DELETE CASCADE
    );
    PRINT 'Tabla ApiKeyPermisos creada con éxito.';
END
GO

-- 5. Crear Tabla ApiKeyAuditoria
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApiKeyAuditoria')
BEGIN
    CREATE TABLE ApiKeyAuditoria (
        ApiKeyAuditoriaId BIGINT IDENTITY(1,1) NOT NULL,
        FechaHora DATETIME2 NOT NULL,
        IntegracionId INT NULL,
        IntegracionNombre NVARCHAR(100) NULL,
        Workflow NVARCHAR(100) NULL,
        ApiKeyId BIGINT NULL,
        ApiKeyNombre NVARCHAR(100) NULL,
        Endpoint NVARCHAR(250) NOT NULL,
        Metodo NVARCHAR(10) NOT NULL,
        IP NVARCHAR(45) NULL,
        CorrelationId NVARCHAR(100) NULL,
        TiempoRespuestaMs INT NOT NULL,
        Resultado NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_ApiKeyAuditoria PRIMARY KEY CLUSTERED (ApiKeyAuditoriaId)
    );
    PRINT 'Tabla ApiKeyAuditoria creada con éxito.';
END
GO

-- 6. Crear Tabla HistorialApiKeys
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistorialApiKeys')
BEGIN
    CREATE TABLE HistorialApiKeys (
        HistorialApiKeyId BIGINT IDENTITY(1,1) NOT NULL,
        ApiKeyId BIGINT NOT NULL,
        Accion NVARCHAR(50) NOT NULL,
        Fecha DATETIME2 NOT NULL,
        RealizadoPor NVARCHAR(100) NOT NULL,
        Detalle NVARCHAR(500) NULL,
        CONSTRAINT PK_HistorialApiKeys PRIMARY KEY CLUSTERED (HistorialApiKeyId),
        CONSTRAINT FK_HistorialApiKeys_ApiKeys FOREIGN KEY (ApiKeyId) REFERENCES ApiKeys(ApiKeyId) ON DELETE CASCADE
    );
    PRINT 'Tabla HistorialApiKeys creada con éxito.';
END
GO
