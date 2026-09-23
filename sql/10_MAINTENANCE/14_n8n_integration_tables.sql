USE SIR_NacionalSeguros;
GO

PRINT 'Evaluando creación de tabla WSSessions...';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WSSessions')
BEGIN
    CREATE TABLE dbo.WSSessions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ChannelIdentifier NVARCHAR(100) NOT NULL,
        NormalizedIdentifier NVARCHAR(100) NOT NULL UNIQUE,
        ActiveAgent NVARCHAR(100) NOT NULL,
        SessionStatus NVARCHAR(50) NOT NULL,
        TemporaryDataJson NVARCHAR(MAX) NULL,
        PendingFieldsJson NVARCHAR(MAX) NULL,
        LastInteractionAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Tabla WSSessions creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla WSSessions ya existe. Omitiendo creación.';
END
GO

PRINT 'Evaluando creación de tabla AgentEvents...';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AgentEvents')
BEGIN
    CREATE TABLE dbo.AgentEvents (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ChannelType NVARCHAR(50) NOT NULL,
        ChannelIdentifier NVARCHAR(100) NOT NULL,
        EventType NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        EventSource NVARCHAR(100) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        MetadataJson NVARCHAR(MAX) NULL,
        RelatedEntityType NVARCHAR(100) NULL,
        RelatedEntityId NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    PRINT 'Tabla AgentEvents creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla AgentEvents ya existe. Omitiendo creación.';
END
GO

PRINT 'Ejecución del script completada.';
GO
